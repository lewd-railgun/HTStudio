using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HTStudio.Project.RPGMV
{
    /// <summary>
    /// RPGMVExtractOptionWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RPGMVExtractOptionWindow : Window
    {
        private RPGMVExtractor extractor;
        public RPGMVExtractor Extractor {
            get {
                return extractor;
            }
            set {
                ExtractEventTextCheckBox.IsChecked = value.ExtractEventText;
                ExtractEventScriptCheckBox.IsChecked = value.ExtractEventScript;
                ExtractCommonEventCheckBox.IsChecked = value.ExtractCommonEvent;
                ExtractMapEventCheckBox.IsChecked = value.ExtractMapEvent;

                ExtractActorsCheckBox.IsChecked = value.ExtractActors;
                ExtractArmorsCheckBox.IsChecked = value.ExtractArmors;
                ExtractEnemiesCheckBox.IsChecked = value.ExtractEnemies;
                ExtractItemsCheckBox.IsChecked = value.ExtractItems;
                ExtractMapInfosCheckBox.IsChecked = value.ExtractMapInfos;
                ExtractSkillsCheckBox.IsChecked = value.ExtractSkills;
                ExtractStatesCheckBox.IsChecked = value.ExtractStates;
                ExtractTroopsCheckBox.IsChecked = value.ExtractTroops;
                ExtractWeaponsCheckBox.IsChecked = value.ExtractWeapons;

                ExtractSystemCheckBox.IsChecked = value.ExtractSystemData;

                extractor = value;
            }
        }

        public RPGMVExtractOptionWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if(ExtractEventTextCheckBox.IsChecked.HasValue)
                extractor.ExtractEventText = ExtractEventTextCheckBox.IsChecked.Value;

            if (ExtractEventScriptCheckBox.IsChecked.HasValue)
                extractor.ExtractEventScript = ExtractEventScriptCheckBox.IsChecked.Value;

            if (ExtractCommonEventCheckBox.IsChecked.HasValue)
                extractor.ExtractCommonEvent = ExtractCommonEventCheckBox.IsChecked.Value;

            if (ExtractMapEventCheckBox.IsChecked.HasValue)
                extractor.ExtractMapEvent = ExtractMapEventCheckBox.IsChecked.Value;

            if (ExtractActorsCheckBox.IsChecked.HasValue)
                extractor.ExtractActors = ExtractActorsCheckBox.IsChecked.Value;

            if (ExtractArmorsCheckBox.IsChecked.HasValue)
                extractor.ExtractArmors = ExtractArmorsCheckBox.IsChecked.Value;

            if (ExtractEnemiesCheckBox.IsChecked.HasValue)
                extractor.ExtractEnemies = ExtractEnemiesCheckBox.IsChecked.Value;

            if (ExtractItemsCheckBox.IsChecked.HasValue)
                extractor.ExtractItems = ExtractItemsCheckBox.IsChecked.Value;

            if (ExtractMapInfosCheckBox.IsChecked.HasValue)
                extractor.ExtractMapInfos = ExtractMapInfosCheckBox.IsChecked.Value;

            if (ExtractSkillsCheckBox.IsChecked.HasValue)
                extractor.ExtractSkills = ExtractSkillsCheckBox.IsChecked.Value;

            if (ExtractStatesCheckBox.IsChecked.HasValue)
                extractor.ExtractStates = ExtractStatesCheckBox.IsChecked.Value;

            if (ExtractTroopsCheckBox.IsChecked.HasValue)
                extractor.ExtractTroops = ExtractTroopsCheckBox.IsChecked.Value;

            if (ExtractWeaponsCheckBox.IsChecked.HasValue)
                extractor.ExtractWeapons = ExtractWeaponsCheckBox.IsChecked.Value;

            if (ExtractSystemCheckBox.IsChecked.HasValue)
                extractor.ExtractSystemData = ExtractSystemCheckBox.IsChecked.Value;

            extractor.SaveOption();
        }
    }
}
