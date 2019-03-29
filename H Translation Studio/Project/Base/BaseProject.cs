using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace HTStudio.Project.Base
{
    //모든 프로젝트의 기반
    public class BaseProject
    {
        public string path;

        public string ProjectPath {
            get {
                return Path.Combine(path, ".HTStudio");
            }
        }

        public string BackupPath {
            get {
                return Path.Combine(ProjectPath, "Backup");
            }
        }


        private bool inHandTranslateMode = false;
        public bool InHandTranslateMode {
            get {
                return inHandTranslateMode;
            }
            set {
                inHandTranslateMode = value;
                Save();
            }
        }

        private int lastWorkIndex = 0;
        public int LastWorkIndex {
            get {
                return lastWorkIndex;
            }
            set {
                lastWorkIndex = value;
                Save();
            }
        }

        private string BaseJsonPath {
            get {
                return Path.Combine(ProjectPath, "base.json");
            }
        }
        private void Save()
        {
            var json = new JObject();
            json["InHandTranslateMode"] = InHandTranslateMode;
            json["LastWorkIndex"] = LastWorkIndex;
            File.WriteAllText(BaseJsonPath, JsonConvert.SerializeObject(json));
        }
        
        public BaseProject(string path)
        {
            this.path = path;
            baseExtractor = new BaseExtractor(this);

            Directory.CreateDirectory(ProjectPath);
            if( File.Exists(BaseJsonPath) )
            {
                var json = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(BaseJsonPath));

                inHandTranslateMode = json["InHandTranslateMode"].ToObject<bool>();
                lastWorkIndex = json["LastWorkIndex"].ToObject<int>();
            }
        }

        public static BaseProject IdentificationProject(string path)
        {
            BaseProject result = null;

            if ((result = RPGMV.RPGMVProject.Identification(path)) != null) return result;

            return new BaseProject(path);
        }

        public virtual string Name {
            get {
                return "알 수 없음";
            }
        }

        private BaseExtractor baseExtractor;
        public virtual BaseExtractor Extractor {
            get {
                return baseExtractor;
            }
        }
    }
}
