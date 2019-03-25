using HTStudio.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HTStudio.Project.Base
{
    public class BaseExtractor
    {
        public string path;

        public BaseExtractor(string path)
        {
            this.path = path;
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

        public virtual List<TranslateString> TranslateStrings {
            get {
                return null;
            }
        }
    }
}
