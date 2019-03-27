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

namespace HTStudio.Project.RPGMV
{
    public class RPGMVExtractor : BaseExtractor
    {
        public RPGMVExtractor(string path) : base(path)
        {
        }

        public override bool SupportExtract => true;

        public override void Extract()
        {
            TranslateStrings.Clear();

            //COMMON EVENT
            JArray events = JArray.Parse(File.ReadAllText(Path.Combine(path, "www/data/CommonEvents.json")));

            foreach(JToken data in events)
            {
                //빈 데이터 방지
                if (data == null || data is JValue) continue;

                string name = data["name"].ToString();
                InsertNewTranslateStrings(name);

                JArray commands = data["list"] as JArray;
                int i = 0;
                while (i < commands.Count) {
                    JToken command = commands[i];
                    int code = command["code"].ToObject<int>();

                    //모든 텍스트 데이터 읽기
                    if(code == 101 || code == 102) //대사 명령
                    {
                        i++;
                        StringBuilder builder = new StringBuilder();
                        while (true)
                        {
                            JToken textData = commands[i];
                            int textDataCode = textData["code"].ToObject<int>();
                            if (textDataCode != 401 && textDataCode != 402)
                            {
                                break;
                            }
                            foreach (JToken str in commands[i]["parameters"])
                            {
                                builder.Append(str.ToString());
                                builder.Append("\r\n");
                            }
                            i++;
                        }
                        builder.Remove(builder.Length - 2, 2);
                        InsertNewTranslateStrings(builder.ToString().Trim());
                        continue;
                    }

                    i++;
                }
            }
            SaveTranslateStrings();
        }
    }
}
