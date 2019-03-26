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
        public string path;

        public BaseExtractor(string path)
        {
            this.path = path;

            if( File.Exists( Path.Combine(path, "HTStr.json") ) )
            {
                translateStrings = JsonConvert.DeserializeObject<List<TranslateString>>( File.ReadAllText(Path.Combine(path, "HTStr.json")) );
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

        public virtual void Apply(List<TranslateString> translateStrings)
        {

        }

        private List<TranslateString> translateStrings = new List<TranslateString>();
        public List<TranslateString> TranslateStrings {
            get {
                return translateStrings;
            }
        }

        protected void InsertNewTranslateStrings(string original)
        {
            if (original.Trim() == "") return;
            translateStrings.Add(new TranslateString() { Original = original, Hand = "", Machine = "" });
        }

        public void SaveTranslateStrings()
        {
            File.WriteAllText(Path.Combine(path, "HTStr.json"), JsonConvert.SerializeObject(TranslateStrings));
        }
    }
}
