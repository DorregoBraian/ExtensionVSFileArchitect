using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Windows;
using Window = System.Windows.Window;


namespace FileArchitectVSIX
{
    /// <summary>
    /// Interaction logic for CreateArchitectureWindow.xaml
    /// </summary>
    public partial class CreateArchitectureWindow : Window
    {
        public CreateArchitectureWindow()
        {
            InitializeComponent();
        }

        private void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));

            if (dte?.Solution == null || dte.Solution.Projects.Count == 0)
            {
                MessageBox.Show("No hay ningún proyecto abierto.");
                return;
            }

            Project project = dte.Solution.Projects.Item(1);

            CreateArchitecture(project);

            MessageBox.Show("Arquitectura creada correctamente.");
        }


        private void CreateFolder(Project project, string folderName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string projectPath = Path.GetDirectoryName(project.FullName);
            string folderPath = Path.Combine(projectPath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                project.ProjectItems.AddFromDirectory(folderPath);
            }
        }

        private void CreateArchitecture(Project project)
        {
            CreateFolder(project, "Domain");
            CreateFolder(project, "Application");
            CreateFolder(project, "Infrastructure");
        }

    }
}
