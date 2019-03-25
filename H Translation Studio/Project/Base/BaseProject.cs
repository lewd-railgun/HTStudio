using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTStudio.Project.Base
{
    //모든 프로젝트의 기반
    public class BaseProject
    {
        public string path;
        
        public BaseProject(string path)
        {
            this.path = path;
            baseExtractor = new BaseExtractor(path);
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
