using HTStudio.Project.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HTStudio.Container;
using System.Text.RegularExpressions;
using HTStudio.Worker;

namespace HTStudio.Project.RPGMV
{
    public class RPGMVExtractor : BaseExtractor
    {
        private readonly Regex ScriptStringExtractor = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""");

        public RPGMVExtractor(BaseProject project) : base(project)
        {
            if (!Directory.Exists(project.BackupPath))
            {
                Backup();
            }
        }

        private void SaveJson(string path, object data)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
        }

        private void WorkJsonData(bool isApply, string path, string applyPath, params string[] extracts)
        {
            JArray datas = JArray.Parse(File.ReadAllText(path));
            foreach (JToken data in datas)
            {
                //빈 데이터 방지
                if (data == null || data is JValue) continue;

                foreach (var extract in extracts)
                {
                    if(isApply)
                    {
                        data[extract] = QueryForTranslate(data[extract].ToString());
                    }
                    else
                    {
                        InsertNewTranslateStrings(data[extract].ToString());
                    }
                }
            }
            
            if(isApply)
            {
                SaveJson(applyPath, datas);
            }
        }

        private void WorkSingleString(bool isApply, JToken data, string key)
        {
            if(isApply)
            {
                data[key] = QueryForTranslate(data[key].ToString());
            }
            else
            {
                InsertNewTranslateStrings(data[key].ToString());
            }
        }

        private string WorkScripts(bool isApply, string script)
        {
            var matches = ScriptStringExtractor.Matches(script);
            
            if(isApply)
            {
                string result = script;
                foreach (Match match in matches)
                {
                    var original = match.Value.Substring(1, match.Value.Length - 2);
                    var replace = QueryForTranslate(original);
                    if(original != replace)
                    {
                        result = result.Replace("\"" + original + "\"", "\"" + replace + "\"");
                    }
                }
                return result;
            }
            else
            {
                foreach (Match match in matches)
                {
                    if( Utils.isJapanese(match.Value) )
                    {
                        //TODO: Apply File Name Filter to Exclude File Name

                        InsertNewTranslateStrings(match.Value.Substring(1, match.Value.Length - 2));
                    }
                }
            }
            return null;
        }

        private void WorkCommands(bool isApply, JArray commands)
        {
            int i = 0;
            while (i < commands.Count)
            {
                JToken command = commands[i];
                if (command == null || command is JValue) continue;
                int code = command["code"].ToObject<int>();

                //모든 텍스트 데이터 읽기
                if (code == 101 || code == 102) //대사 명령
                {
                    i++;
                    int startInx = i;
                    StringBuilder builder = new StringBuilder();
                    while (true)
                    {
                        JToken textData = commands[i];
                        int textDataCode = textData["code"].ToObject<int>();
                        if (textDataCode != 401 && textDataCode != 402)
                        {
                            break;
                        }
                        builder.Append(commands[i]["parameters"][0]);
                        builder.Append("\r\n");
                        i++;
                    }
                    builder.Remove(builder.Length - 2, 2);
                        
                    var result = builder.ToString().Trim();

                    if (isApply)
                    {
                        var replace = QueryForTranslate(result);
                        if(replace != result)
                        {
                            string[] lines = replace.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            int inx = 0;
                            i = startInx;
                            while (true)
                            {
                                JToken textData = commands[i];
                                int textDataCode = textData["code"].ToObject<int>();
                                if (textDataCode != 401 && textDataCode != 402)
                                {
                                    break;
                                }

                                string change;
                                if (lines.Length <= inx)
                                {
                                    change = "";
                                }
                                else
                                {
                                    change = lines[inx];
                                }

                                commands[i]["parameters"] = JArray.FromObject(new string[] { change });//지금까지 발견된 바로는, 한 데이터밖에 가지지 않음
                                i++;
                                inx++;
                            }
                        }
                    }
                    else
                    {
                        InsertNewTranslateStrings(result);
                    }

                    continue;
                }
                else if(code == 355) //Script
                {
                    int startInx = i; //스크립트 구문은 본문도 데이터를 포함하고 있기 때문에 같이 고쳐야함!
                    i++;
                    StringBuilder builder = new StringBuilder();
                    builder.Append(command["parameters"][0].ToString());
                    builder.Append("\r\n");
                    while (true)
                    {
                        JToken scriptData = commands[i];
                        int scriptDataCode = scriptData["code"].ToObject<int>();
                        if (scriptDataCode != 655)
                        {
                            break;
                        }
                        builder.Append(commands[i]["parameters"][0]);
                        builder.Append("\r\n");
                        i++;
                    }

                    if ( isApply )
                    {
                        var result = WorkScripts(isApply, builder.ToString());
                        if(result != builder.ToString())
                        {
                            string[] lines = result.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            int inx = 0;
                            i = startInx;
                            while (true)
                            {
                                JToken scriptData = commands[i];
                                int scriptDataCode = scriptData["code"].ToObject<int>();
                                if (scriptDataCode != 355 && scriptDataCode != 655)
                                {
                                    break;
                                }

                                string change;
                                if (lines.Length <= inx)
                                {
                                    change = "";
                                }
                                else
                                {
                                    change = lines[inx];
                                }

                                commands[i]["parameters"] = JArray.FromObject(new string[] { change });//지금까지 발견된 바로는, 한 데이터밖에 가지지 않음
                                i++;
                                inx++;
                            }
                        }
                    }
                    else
                    {
                        WorkScripts(isApply, builder.ToString());
                    }
                }

                i++;
            }
        }

        private void WorkJArray(bool isApply, JArray array)
        {
            for(int i = 0; i < array.Count; i++)
            {
                if(isApply)
                {
                    array[i] = QueryForTranslate(array[i].ToString());
                }
                else
                {
                    InsertNewTranslateStrings(array[i].ToString());
                }
            }
        }

        private void Work(bool isApply)
        {
            var backupDataPath = Path.Combine(project.BackupPath, "www/data");
            var applyDataPath = Path.Combine(project.path, "www/data");

            WorkJsonData(isApply, Path.Combine(backupDataPath, "Actors.json"), Path.Combine(applyDataPath, "Actors.json"), "name");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "Armors.json"), Path.Combine(applyDataPath, "Armors.json"), "name", "description");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "Enemies.json"), Path.Combine(applyDataPath, "Enemies.json"), "name");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "Items.json"), Path.Combine(applyDataPath, "Items.json"), "name", "description");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "MapInfos.json"), Path.Combine(applyDataPath, "MapInfos.json"), "name");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "Skills.json"), Path.Combine(applyDataPath, "Skills.json"), "name", "description");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "States.json"), Path.Combine(applyDataPath, "States.json"), "name", "message1", "message2", "message3", "message4");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "Troops.json"), Path.Combine(applyDataPath, "Troops.json"), "name");
            WorkJsonData(isApply, Path.Combine(backupDataPath, "Weapons.json"), Path.Combine(applyDataPath, "Weapons.json"), "name", "description");

            //COMMON EVENT
            var commonEvents = JArray.Parse(File.ReadAllText(Path.Combine(backupDataPath, "CommonEvents.json")));
            foreach(var commonEvent in commonEvents)
            {
                if (commonEvent == null || commonEvent is JValue) continue;
                WorkSingleString(isApply, commonEvent, "name");
                WorkCommands(isApply, commonEvent["list"] as JArray);
            }
            if(isApply)
            {
                SaveJson(Path.Combine(applyDataPath, "CommonEvents.json"), commonEvents);
            }

            //MAP
            foreach (var path in Directory.GetFiles(backupDataPath))
            {
                var lowerName = Path.GetFileName(path).ToLower();
                if (lowerName.StartsWith("map") && !lowerName.StartsWith("mapinfos"))
                {
                    var map = JObject.Parse(File.ReadAllText(path));
                    foreach (JToken data in map["events"])
                    {
                        //빈 데이터 방지
                        if (data == null || data is JValue) continue;

                        WorkSingleString(isApply, data, "name");
                        foreach (var page in data["pages"])
                        {
                            WorkCommands(isApply, page["list"] as JArray);
                        }
                    }

                    if(isApply)
                    {
                        SaveJson( Path.Combine(applyDataPath, Path.GetFileName(path)), map);
                    }
                }
            }

            //SYSTEM DATA
            JObject system = JObject.Parse(File.ReadAllText(Path.Combine(backupDataPath, "System.json")));
            WorkJArray(isApply, system["armorTypes"] as JArray);
            WorkJArray(isApply, system["elements"] as JArray);
            WorkJArray(isApply, system["equipTypes"] as JArray);
            WorkJArray(isApply, system["skillTypes"] as JArray);
            WorkJArray(isApply, system["switches"] as JArray);
            WorkJArray(isApply, system["variables"] as JArray);
            WorkJArray(isApply, system["weaponTypes"] as JArray);

            var terms = system["terms"];
            WorkJArray(isApply, terms["basic"] as JArray);
            WorkJArray(isApply, terms["commands"] as JArray);
            WorkJArray(isApply, terms["params"] as JArray);

            foreach (var item in (terms["messages"] as JObject).Properties())
            {
                WorkSingleString(isApply, terms["messages"], item.Name);
            }

            WorkSingleString(isApply, system, "currencyUnit");
            WorkSingleString(isApply, system, "gameTitle");
            WorkSingleString(isApply, system, "title1Name");
            WorkSingleString(isApply, system, "title2Name");

            if (isApply)
            {
                SaveJson(Path.Combine(applyDataPath, "System.json"), system);
            }
        }
        
        public override bool SupportExtract => true;

        public override bool SupportApply => true;

        public override void Extract()
        {
            TranslateStrings.Clear();

            Work(false);

            SaveTranslateStrings();
        }

        public override void Backup()
        {
            var dataPath = Path.Combine(ProjectPath, "www/data");
            Utils.DirectoryCopy(dataPath, Path.Combine(project.BackupPath, "www/data"), true);
        }

        public override void Apply()
        {
            Work(true);

        }
    }
}
