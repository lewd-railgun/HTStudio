﻿using System;
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
            StartLongProgress();
            workingDirectory = directory;

            WorkingTextBlock.Text = "작업 중: " + Path.GetFileName(directory);
            project = BaseProject.IdentificationProject(directory);

            ProjectTypeTextBlock.Text = "프로젝트 타입 : " + project.Name;

            ExtractStringButton.IsEnabled = project.Extractor.SupportExtract;
            ApplyStringButton.IsEnabled = project.Extractor.SupportApply;
            ExtractorOptionButton.IsEnabled = project.Extractor.HasWindow;

            if( File.Exists(AutoSavePath) )
            {
                if(MessageBox.Show("예기치 못한 종료전에 저장된 자동 저장 파일이 남아있습니다. 이 파일로 작업합니까?", "HT Studio", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    project.Extractor.LoadTranslateStrings(AutoSavePath);
                }
                try
                {
                    File.Delete(AutoSavePath);
                }
                catch
                {

                }
            }

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

            if (StringListBox.Items.Count > 0)
            {
                StringListBox.SelectedIndex = project.LastWorkIndex;
            }
            ActivateHandCheckBox.IsChecked = project.InHandTranslateMode;

            FinishLongProgress();
        }

        private RoutedCommand FocusMachineHotKey = new RoutedCommand();
        private RoutedCommand FocusHandHotKey = new RoutedCommand();

        private RoutedCommand PrevStringHotKey = new RoutedCommand();
        private RoutedCommand NextStringHotKey = new RoutedCommand();
        private RoutedCommand NeedWorkStringHotKey = new RoutedCommand();

        private bool isEdited = false;

        private string AutoSavePath {
            get {
                return Path.Combine(project.ProjectPath, "TranslateStringsAutoSave.json");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            FocusMachineHotKey.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            FocusHandHotKey.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));

            PrevStringHotKey.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            NextStringHotKey.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            NeedWorkStringHotKey.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));

            CommandBindings.Add(new CommandBinding(FocusMachineHotKey, FocusToMachine));
            CommandBindings.Add(new CommandBinding(FocusHandHotKey, FocusToHand));

            CommandBindings.Add(new CommandBinding(PrevStringHotKey, PrevStringButton_Click));
            CommandBindings.Add(new CommandBinding(NextStringHotKey, NextStringButton_Click));
            CommandBindings.Add(new CommandBinding(NeedWorkStringHotKey, NeedWorkStringButton_Click));

            var startTimeSpan = TimeSpan.FromMinutes(5);
            var periodTimeSpan = TimeSpan.FromMinutes(5);

            var timer = new Timer((e) =>
            {
                try
                {
                    if (project == null) return;
                    project.Extractor.SaveTranslateStrings(AutoSavePath);
                }
                catch
                {

                }
            }, null, startTimeSpan, periodTimeSpan);
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

            project.LastWorkIndex = StringListBox.SelectedIndex;
            var item = StringListBox.Items[StringListBox.SelectedIndex] as TranslateString;

            OriginalTextBox.Text = item.Original;
            MachineTextBox.Text = item.Machine;
            HandTextBox.Text = item.Hand;
        }

        private void SaveTranslateStringButton_Click(object sender, RoutedEventArgs e)
        {
            isEdited = false;
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
            isEdited = true;
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
            isEdited = true;
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
            project.Extractor.SaveTranslateStrings();
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

        private void FocusToMachine(object sender, ExecutedRoutedEventArgs e)
        {
            MachineTextBox.Focus();
        }

        private void FocusToHand(object sender, ExecutedRoutedEventArgs e)
        {
            HandTextBox.Focus();
        }

        private void PrevStringButton_Click(object sender, RoutedEventArgs e)
        {
            if (StringListBox.SelectedIndex <= 0) return;

            StringListBox.SelectedIndex--;
        }

        private void NextStringButton_Click(object sender, RoutedEventArgs e)
        {
            if (StringListBox.SelectedIndex > StringListBox.Items.Count) return;

            StringListBox.SelectedIndex++;
        }

        private void NeedWorkStringButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var translateString in project.Extractor.TranslateStrings.Select((value, i) => new { i, value }))
            {
                if(translateString.value.Machine == "" || (translateString.value.Hand == "" && project.InHandTranslateMode) )
                {
                    StringListBox.SelectedIndex = translateString.i;
                    StringListBox.ScrollIntoView(translateString.value);
                    return;
                }
            }

            MessageBox.Show("모든 작업이 완료된것으로 보입니다");
        }

        private void ActivateHandCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("손 번역을 시작하시기로 하신것을 감사하게 생각합니다.\r\n손 번역창을 채운경우 손 번역이, 아닌경우 기계 번역이 자동 적용됩니다!");
            HandTextBox.IsEnabled = true;
            project.InHandTranslateMode = true;
        }

        private void ActivateHandCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("손 번역 기능이 중단되었습니다만 이전에 손 번역 하신 내용은 유지됩니다.\r\n마찬가지로 손 번역창을 채운경우 손 번역이, 아닌경우 기계 번역이 자동 적용됩니다!\r\n이를 초기화 하고 싶으시면 .HTStudio 폴더를 삭제후 다시 프로젝트를 생성하시거나 수동으로 지워주세요.");
            HandTextBox.IsEnabled = false;
            project.InHandTranslateMode = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if( isEdited && MessageBox.Show("변경된 데이터가 일부 저장되지 않았을 가능성이 있습니다, 그래도 닫습니까?", "HT Studio", MessageBoxButton.YesNo) == MessageBoxResult.No )
            {
                e.Cancel = true;
                return;
            }

            try
            {
                File.Delete(AutoSavePath);
            }
            catch(Exception ex)
            {
                
            }
        }
    }
}
