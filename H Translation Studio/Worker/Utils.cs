using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HTStudio.Worker
{
    public static class Utils
    {
        private static Regex japaneseFinder = new Regex("[(一-龿|぀-ゟ|゠-ヿ)]");
        public static bool isJapanese(string word)
        {
            return japaneseFinder.IsMatch(word);
        }
    }
}
