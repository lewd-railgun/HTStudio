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

namespace HTStudio.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private string workingDirectory;

        private BaseProject project;

        public void startWorkWith(string directory)
        {
            workingDirectory = directory;

            WorkingTextBlock.Text = "작업 중: " + Path.GetFileName(directory);
            project = BaseProject.IdentificationProject(directory);

            ProjectTypeTextBlock.Text = "프로젝트 타입 : " + project.Name;

            ExtractStringButton.IsEnabled = project.Extractor.SupportExtract;
            ApplyStringButton.IsEnabled = project.Extractor.SupportApply;
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
