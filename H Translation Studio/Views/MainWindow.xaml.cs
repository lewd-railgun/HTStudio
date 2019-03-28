using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using HTStudio.Project.Base;
using HTStudio.Container;
using HTStudio.Worker;
using System.Threading;

namespace HTStudio.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private void StartLongProgress()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                IsEnabled = false;
                ProgressTextBlock.Text = "작업 초기화...";
            }));
        }

        private void UpdateLongProgressMessage(string text)
        {
            ProgressTextBlock.Dispatcher.Invoke(new Action(() =>
            {
                ProgressTextBlock.Text = text;
            }));
        }

        private void FinishLongProgress()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                IsEnabled = true;
                ProgressTextBlock.Text = "대기";
            }));
        }

        private string workingDirectory;

        private BaseProject project;

        private long machineCount;
        private long handCount;

        public void UpdateTranslateState(bool refresh = false)
        {
            if(refresh)
            {
                machineCount = 0;
                handCount = 0;
                foreach (TranslateString str in StringListBox.Items)
                {
                    if (str.Machine.Trim() != "")
                    {
                        machineCount++;
                    }

                    if (str.Hand.Trim() != "")
                    {
                        handCount++;
                    }
                }
            }
            TranslationStatusTextBlock.Text = "원본/번역기/손번역 상태 : " + StringListBox.Items.Count + "/" + machineCount + "/" + handCount;
        }

        public void startWorkWith(string directory)
        {
            workingDirectory = directory;

            WorkingTextBlock.Text = "작업 중: " + Path.GetFileName(directory);
            project = BaseProject.IdentificationProject(directory);

            ProjectTypeTextBlock.Text = "프로젝트 타입 : " + project.Name;

            ExtractStringButton.IsEnabled = project.Extractor.SupportExtract;
            ApplyStringButton.IsEnabled = project.Extractor.SupportApply;
            ExtractorOptionButton.IsEnabled = project.Extractor.HasWindow;

            foreach (TranslateString str in project.Extractor.TranslateStrings)
            {
                StringListBox.Items.Add(str);
                if(str.Machine.Trim() != "")
                {
                    machineCount++;
                }

                if(str.Hand.Trim() != "")
                {
                    handCount++;
                }
            }
            UpdateTranslateState();


        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExtractStringButton_Click(object sender, RoutedEventArgs e)
        {
            StringListBox.Items.Clear();
            project.Extractor.Extract();
            foreach(TranslateString str in project.Extractor.TranslateStrings)
            {
                StringListBox.Items.Add(str);
            }
            UpdateTranslateState();
        }

        private void StringListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StringListBox.SelectedIndex == -1) return;

            var item = StringListBox.Items[StringListBox.SelectedIndex] as TranslateString;

            OriginalTextBox.Text = item.Original;
            MachineTextBox.Text = item.Machine;
            HandTextBox.Text = item.Hand;
        }

        private void SaveTranslateStringButton_Click(object sender, RoutedEventArgs e)
        {
            project.Extractor.SaveTranslateStrings();
        }

        private void MachineTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StringListBox.SelectedIndex == -1) return;

            var item = StringListBox.Items[StringListBox.SelectedIndex] as TranslateString;

            if(item.Machine == "" && MachineTextBox.Text != "")
            {
                machineCount++;
                UpdateTranslateState();
            }
            else if(item.Machine != "" && MachineTextBox.Text == "")
            {
                machineCount--;
                UpdateTranslateState();
            }

            item.Machine = MachineTextBox.Text;

        }

        private void HandTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StringListBox.SelectedIndex == -1) return;

            var item = StringListBox.Items[StringListBox.SelectedIndex] as TranslateString;

            if (item.Hand == "" && HandTextBox.Text != "")
            {
                handCount++;
                UpdateTranslateState();
            }
            else if (item.Hand != "" && HandTextBox.Text == "")
            {
                handCount--;
                UpdateTranslateState();
            }

            item.Hand = HandTextBox.Text;
        }

        private void AutoTransButton_Click(object sender, RoutedEventArgs e)
        {
            if( MessageBox.Show("저장된 기계 번역을 전부 초기화하고 처음부터 다시합니까?", "HT Studio", MessageBoxButton.YesNo) == MessageBoxResult.Yes )
            {
                foreach(var translateString in project.Extractor.TranslateStrings)
                {
                    translateString.Machine = "";
                }
            }

            new Thread(AutoTrans).Start();
        }

        private void AutoTrans() //EZTransXP Doesn't support multi-threading :/
        {
            StartLongProgress();
            try
            {
                UpdateLongProgressMessage("EZTransXP 모듈 초기화...");
                if (!EZTransXP.IsInited)
                    EZTransXP.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                FinishLongProgress();
                return;
            }
            
            long lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 500;
            for (int i = 0; i < StringListBox.Items.Count; i++)
            {
                TranslateString str = StringListBox.Items[i] as TranslateString;
                if (str.Machine != "") continue;
                try
                {
                    if (lastUpdate + 500 < DateTimeOffset.Now.ToUnixTimeMilliseconds()) {
                        UpdateLongProgressMessage("번역중 " + i + "/" + StringListBox.Items.Count);
                        lastUpdate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                    str.Machine = EZTransXP.TranslateJ2K(str.Original);
                    machineCount++;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            Dispatcher.Invoke(new Action(() =>
            {
                UpdateTranslateState(true);
            }));
            FinishLongProgress();
        }

        private void ApplyStringButton_Click(object sender, RoutedEventArgs e)
        {
            project.Extractor.Apply();
            MessageBox.Show("적용이 완료되었습니다!");
        }

        private void ExtractorOptionButton_Click(object sender, RoutedEventArgs e)
        {
            project.Extractor.CreateWindow().ShowDialog();
        }

        private void RestoreOriginalButton_Click(object sender, RoutedEventArgs e)
        {
            project.Extractor.Restore();
            MessageBox.Show("원본 복구가 완료되었습니다!");
        }
    }
}
