using HTStudio.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Newtonsoft.Json;

namespace HTStudio.Project.Base
{
    public class BaseExtractor
    {
        public BaseProject project;

        public string ProjectPath {
            get {
                return project.ProjectPath;
            }
        }

        private string StringsPath {
            get {
                return Path.Combine(ProjectPath, "TranslateStrings.json");
            }
        }

        public BaseExtractor(BaseProject project)
        {
            this.project = project;

            if(File.Exists(StringsPath))
            {
                TranslateStrings = JsonConvert.DeserializeObject<List<TranslateString>>(File.ReadAllText(StringsPath) );
                foreach(var ts in TranslateStrings)
                {
                    TranslateStringDict[ts.Original] = ts;
                }
            }
        }

        public virtual bool HasWindow {
            get {
                return false;
            }
        }

        public virtual bool SupportExtract {
            get {
                return false;
            }
        }

        public virtual bool SupportApply {
            get {
                return false;
            }
        }

        public virtual Window CreateWindow() {
            return null;
        }

        public virtual void Extract()
        {

        }

        public virtual void Backup()
        {

        }

        public virtual void Apply()
        {

        }
        
        public List<TranslateString> TranslateStrings { get; } = new List<TranslateString>();

        private Dictionary<String, TranslateString> TranslateStringDict { get; } = new Dictionary<String, TranslateString>();

        protected string QueryForTranslate(string original)
        {
            if(TranslateStringDict.ContainsKey(original))
            {
                var translate = TranslateStringDict[original];
                if(translate.Hand != "")
                {
                    return translate.Hand;
                }
                if(translate.Machine != "")
                {
                    return translate.Machine;
                }
            }

            return original;
        }

        protected void InsertNewTranslateStrings(string original)
        {
            if (original.Trim() == "") return;
            if (TranslateStringDict.ContainsKey(original)) return;
            var ts = new TranslateString() { Original = original, Hand = "", Machine = "" };
            TranslateStrings.Add(ts);
            TranslateStringDict[original] = ts;
        }

        public void SaveTranslateStrings()
        {
            File.WriteAllText(System.IO.Path.Combine(ProjectPath, "HTStr.json"), JsonConvert.SerializeObject(TranslateStrings));
        }
    }
}
