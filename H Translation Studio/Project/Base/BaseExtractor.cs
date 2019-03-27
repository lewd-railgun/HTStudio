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
                TranslateStrings = JsonConvert.DeserializeObject<List<TranslateString>>( File.ReadAllText(Path.Combine(path, "HTStr.json")) );
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

        public virtual void Apply()
        {

        }
        public List<TranslateString> TranslateStrings { get; } = new List<TranslateString>();

        private Dictionary<String, TranslateString> TranslateStringDict { get; } = new Dictionary<String, TranslateString>();

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
            File.WriteAllText(Path.Combine(path, "HTStr.json"), JsonConvert.SerializeObject(TranslateStrings));
        }
    }
}
