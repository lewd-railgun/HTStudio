using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTStudio.Project.Base;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HTStudio.Worker;

namespace HTStudio.Project.RPGMV.Dieselmine
{
    public class RPGMVDieselmineExtractor : RPGMVExtractor
    {
        public RPGMVDieselmineExtractor(BaseProject project) : base(project)
        {

        }

        public override void Backup()
        {
            base.Backup();
            Utils.DirectoryCopy(Path.Combine(project.path, "scenario"), Path.Combine(project.BackupPath, "scenario"), true);
        }

        private char[] ApplyXOR(char[] original, int key)
        {
            for (int i = 0; i < original.Length; i++)
            {
                original[i] = (char)(original[i] ^ key);
            }

            return original;
        }

        private void WorkWithScenarioes(bool isApply)
        {
            //디젤마인은 XOR 난독화를 시나리오 파일에 사용함, 일단 풀어야 뭘 지지고 복고 하는게 가능
            var scenarioPath = Path.Combine(project.BackupPath, "scenario");
            var scenarioApplyPath = Path.Combine(project.path, "scenario");
            //var unpackPath = Path.Combine(project.ProjectPath, "scenario");
            //Directory.CreateDirectory(unpackPath);

            int key = 255;

            try //Try to get key for decrypt
            {
                var pluginsLines = File.ReadAllLines(Path.Combine(project.path, "www/js/plugins.js"));
                foreach (var pluginInfo in pluginsLines)
                {
                    if (pluginInfo.Contains("TS_Decode"))
                    {
                        //{"name":"TS_Decode","status":true,"description":"シナリオファイルデコード","parameters":{"Decode":"true","Key":"255"}}, ->
                        //{"name":"TS_Decode","status":true,"description":"シナリオファイルデコード","parameters":{"Decode":"true","Key":"255"}}
                        var json = JsonConvert.DeserializeObject<JObject>(pluginInfo.Substring(0, pluginInfo.Length - 1));

                        key = json["parameters"]["Key"].ToObject<int>();
                    }
                }
            }
            catch
            {

            }

            Dictionary<string, string> decryptedScenarioes = new Dictionary<string, string>();

            var utf8 = Encoding.GetEncoding("UTF-8");
            foreach (var encryptedFilePath in Directory.GetFiles(scenarioPath))
            {
                var name = Path.GetFileName(encryptedFilePath);

                var workingChar = utf8.GetChars(File.ReadAllBytes(encryptedFilePath));
                decryptedScenarioes.Add(name, new string( ApplyXOR(workingChar, key) ));
                //File.WriteAllText(Path.Combine(unpackPath, Path.GetFileName(encryptedFilePath)), new string(workingChar));
            }
            
            foreach(var scriptData in decryptedScenarioes)
            {
                var script = scriptData.Value;
                var split = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for(int i = 0; i < split.Length; i++)
                {
                    var line = split[i];

                    if( line.Trim() == "")
                    {
                        continue;
                    }

                    if( line.StartsWith(";;") || line.StartsWith("@") || line.StartsWith("*") )
                    {
                        continue;
                    }

                    //CSV based data - skip;
                    if( line.Contains(",") )
                    {
                        continue;
                    }

                    //Has Name!
                    if (line.StartsWith("["))
                    {
                        var name = line.Substring(1, line.IndexOf(']')-1);
                        var extraName = ""; //Voice FN?
                        var text = line.Substring(line.IndexOf(']') + 1);

                        if(name.Contains("/"))
                        {
                            var nameSplit = name.Split('/');
                            name = nameSplit[0];
                            extraName = "/" + nameSplit[1];
                        }

                        if(isApply)
                        {
                            var result = line.Replace(name + extraName, QueryForTranslate(name) + extraName);
                            result = line.Replace(text, QueryForTranslate(text));
                            split[i] = result;
                        }
                        else
                        {
                            InsertNewTranslateStrings(name);
                            InsertNewTranslateStrings(text);
                        }
                    }
                    else
                    {
                        if(isApply)
                        {
                            split[i] = QueryForTranslate(line);
                        }
                        else
                        {
                            InsertNewTranslateStrings(line);
                        }
                    }
                }

                if(isApply)
                {
                    var result = new StringBuilder();
                    foreach(var line in split)
                    {
                        result.Append(line);
                        result.Append("\r\n");
                    }

                    File.WriteAllBytes(Path.Combine(scenarioApplyPath, scriptData.Key), Encoding.UTF8.GetBytes(ApplyXOR(result.ToString().ToCharArray(), key)));
                }
            }
        }

        public override void Extract()
        {
            base.Extract();
            WorkWithScenarioes(false);
        }

        public override void Apply()
        {
            base.Apply();
            WorkWithScenarioes(true);
        }

        public override void Restore()
        {
            base.Restore();
            Utils.DirectoryCopy(Path.Combine(project.BackupPath, "scenario"), Path.Combine(project.path, "scenario"), true, true);
        }
    }
}
