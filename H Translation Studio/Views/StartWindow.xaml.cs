using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace HTStudio.Views
{
    /// <summary>
    /// StartPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class StartWindow : Window
    {
        private string lastPathFile = "lastPath.txt";

        public StartWindow()
        {
            InitializeComponent();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            VersionTextBlock.Text = "Version " + version;
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Select Project Directory
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if(result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.startWorkWith(dialog.SelectedPath);
            mainWindow.Show();
            Close();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if( !File.Exists(lastPathFile) )
            {
                MessageBox.Show("최근에 열었던 프로젝트가 없습니다");
                return;
            }
            //TODO: Open Last Project
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
