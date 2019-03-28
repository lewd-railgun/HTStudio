using HTStudio.Project.Base;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTStudio.Project.RPGMV
{
    public class RPGMVProject : BaseProject
    {
        public RPGMVProject(string path) : base(path)
        {
            extractor = new RPGMVExtractor(this);
        }

        private RPGMVExtractor extractor;

        public override string Name => "RPG MV";

        public override BaseExtractor Extractor => extractor;

        public static RPGMVProject Identification(string path)
        {
            if (!File.Exists( Path.Combine(path, "Game.exe"))) return null;
            if (!File.Exists( Path.Combine(path, "www/data/System.json"))) return null;

            return new RPGMVProject(path);
        }
    }
}
