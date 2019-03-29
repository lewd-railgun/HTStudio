using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HTStudio.Project.Base;

namespace HTStudio.Project.RPGMV.Dieselmine
{
    public class RPGMVDieselmineProject : RPGMVProject
    {
        public RPGMVDieselmineProject(string path) : base(path)
        {
            extractor = new RPGMVDieselmineExtractor(this);
        }

        public override string Name => "RPG MV (+디젤마인 ADV 엔진)";

        private RPGMVDieselmineExtractor extractor;
        public override BaseExtractor Extractor => extractor;

        public new static RPGMVProject Identification(string path)
        {
            if (!File.Exists(Path.Combine(path, "Game.exe"))) return null;
            if (!File.Exists(Path.Combine(path, "www/data/System.json"))) return null;
            if (!File.Exists(Path.Combine(path, "www/js/plugins/TS_ADVsystem.js"))) return null;
            if (!Directory.Exists(Path.Combine(path, "scenario"))) return null;

            var project = new RPGMVDieselmineProject(path);
            if (!Directory.Exists(project.BackupPath))
            {
                project.extractor.Backup();
            }
            return project;
        }
    }
}
