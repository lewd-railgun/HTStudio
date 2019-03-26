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
                foreach (JToken command in commands) {
                    int code = command["code"].ToObject<int>();

                    if(code == 401 || code == 402) //대사 텍스트
                    {
                        foreach(JToken str in command["parameters"])
                        {
                            InsertNewTranslateStrings(str.ToString());
                        }
                    }
                }
            }
        }
    }
}
