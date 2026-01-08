using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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

        // Método para manejar el clic en el botón "Generar"
        private void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Obtener la arquitectura seleccionada
            string architecture = GetSelectedArchitecture();

            if (string.IsNullOrEmpty(architecture))
            {
                MessageBox.Show("Seleccioná una arquitectura.");
                return;
            }

            // Obtener el proyecto actual
            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));

            if (dte?.Solution == null || dte.Solution.Projects.Count == 0)
            {
                MessageBox.Show("No hay ningún proyecto abierto.");
                return;
            }

            Project project = dte.Solution.Projects.Item(1);

            // Crear la arquitectura de carpetas
            GenerateArchitecture(project, dte, architecture);

            MessageBox.Show("Arquitectura creada correctamente.");
        }

        // Método para generar la arquitectura seleccionada
        private void GenerateArchitecture(Project project, DTE2 dte, string architecture)
        {
            switch (architecture)
            {
                case "Hexagonal":
                    CreateHexagonalArchitecture(project, dte);
                    //CreateFolder(project, "Domain");
                    //CreateFolder(project, "Application");
                    //CreateFolder(project, "Infrastructure");
                    //CreateFolder(project, "Adapters");
                    break;

                case "Clean":
                    CreateFolder(project, "Core");
                    CreateFolder(project, "UseCases");
                    CreateFolder(project, "InterfaceAdapters");
                    CreateFolder(project, "Frameworks");
                    break;

                case "MVC":
                    CreateFolder(project, "Models");
                    CreateFolder(project, "Views");
                    CreateFolder(project, "Controllers");
                    break;

                default:
                    MessageBox.Show("Seleccioná una arquitectura.");
                    break;
            }
        }

        // Método específico para arquitectura Hexagonal
        private void CreateHexagonalArchitecture(Project project, DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // var dte = (DTE2)Package.GetGlobalService(typeof(DTE)); // Obtiene el DTE2
            var solution = (Solution2)dte.Solution; // Obtiene la solución actual

            try
            {
                // DOMAIN
                var domain = CreateClassLibraryProject(solution, "Domain"); // Domain
                CreateFolder(domain, "Entities"); // Entities
                CreateFolder(domain, "IServices"); // IServices

                // Repository (si se selecciona)
                if (RepositoryCheck.IsChecked == true)
                {
                    CreateFolder(domain, "IRepository"); // Repository
                }


                // APPLICATION
                var application = CreateClassLibraryProject(solution, "Application"); // Application
                CreateFolder(application, "DTOs"); // DTOs
                CreateFolder(application, "IServices"); // Services


                // INFRASTRUCTURE
                var infrastructure = CreateClassLibraryProject(solution, "Infrastructure");
                CreateFile(infrastructure, "DbContext");

                // Repository (si se selecciona)
                if (RepositoryCheck.IsChecked == true)
                {
                    CreateFolder(infrastructure, "Repository"); // Repository
                }

                // CQRS (solo si se selecciona)
                if (CqrsCheck.IsChecked == true)
                {
                    var commands = CreateFolder(infrastructure, "Commands");
                    CreateSubFolder(commands, "Create");
                    CreateSubFolder(commands, "Update");
                    CreateSubFolder(commands, "Delete");

                    var queries = CreateFolder(infrastructure, "Queries");
                    CreateSubFolder(queries, "Get");
                }

                // AutoMapper (si se selecciona)
                if (AutoMapperCheck.IsChecked == true)
                {
                    CreateFile(infrastructure, "AutoMapperProfiles"); // AutoMapper
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al creando estructura: {ex.Message}");
            }
        }

        // Método para crear un proyecto de Class Library
        private Project CreateClassLibraryProject(Solution2 solution,string projectName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Obtiene el template oficial de Class Library de VS
            string templatePath = solution.GetProjectTemplate("ClassLibrary.zip","CSharp");

            // Ruta física donde se va a crear el proyecto
            string solutionDir = Path.GetDirectoryName(solution.FullName);
            string projectDir = Path.Combine(solutionDir, projectName);

            // Crea el proyecto dentro de la solución
            solution.AddFromTemplate(
                templatePath,
                projectDir,
                projectName,
                false);

            // Devuelve el proyecto recién creado
            return solution.Projects
                .Cast<Project>()
                .First(p => p.Name == projectName);
        }

        // Método para crear una carpeta en el proyecto
        private ProjectItem CreateFolder(Project project, string folderName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string projectPath = Path.GetDirectoryName(project.FullName);
            string folderPath = Path.Combine(projectPath, folderName);

            // Crear carpeta en disco si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Agregarla al proyecto de Visual Studio
            return project.ProjectItems.AddFromDirectory(folderPath);
        }

        // Método para crear una subcarpeta dentro de una carpeta existente
        private ProjectItem CreateSubFolder(ProjectItem parentFolder, string folderName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string parentPath = Path.GetDirectoryName(parentFolder.FileNames[1]);
            string folderPath = Path.Combine(parentPath, folderName);

            // Crear carpeta en disco
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Agregar al proyecto dentro de la carpeta padre
            return parentFolder.ProjectItems.AddFromDirectory(folderPath);
        }

        // Método para crear un archivo dentro de una carpeta existente
        private void CreateFile(Project project, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string folderPath = Path.GetDirectoryName(project.FileName);
            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            // Crear archivo si no existe
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $@"namespace {project.Name}
                {{
                    public class {fileName}
                    {{
                    }}
                }}");
            }

            // Agregar al proyecto
            project.ProjectItems.AddFromFile(filePath);
        }

        // Método para resetear la ventana
        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            // Reset arquitectura
            ArchitectureCombo.SelectedIndex = 0;

            // Reset patrones
            RepositoryCheck.IsChecked = false;
            CqrsCheck.IsChecked = false;
            UnitOfWorkCheck.IsChecked = false;
            AutoMapperCheck.IsChecked = false;

            // Limpiar preview
            PreviewTree.Items.Clear();
        }










        // Método para obtener la arquitectura seleccionada en el ComboBox
        private string GetSelectedArchitecture()
        {
            if (ArchitectureCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content.ToString();
            }

            return null;
        }

        // Evento para actualizar la vista previa cuando cambia la selección
        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        // Método para actualizar la vista previa del TreeView
        private void UpdatePreview()
        {
            if (PreviewTree == null)
                return;

            PreviewTree.Items.Clear();

            if (ArchitectureCombo.SelectedIndex <= 0)
                return;

            string architecture =((ComboBoxItem)ArchitectureCombo.SelectedItem).Content.ToString();

            var root = new TreeViewItem
            {
                Header = "Project"
            };

            PreviewTree.Items.Add(root);

            if (architecture == "Hexagonal")
            {
                AddFolder(root, "Domain");
                AddFolder(root, "Application");
                AddFolder(root, "Infrastructure");
            }

            if (architecture == "Clean")
            {
                AddFolder(root, "Core");
                AddFolder(root, "UseCases");
                AddFolder(root, "InterfaceAdapters");
                AddFolder(root, "Frameworks");
            }

            if (architecture == "MVC")
            {
                AddFolder(root, "Models");
                AddFolder(root, "Views");
                AddFolder(root, "Controllers");
            }

            // Patrones
            if (RepositoryCheck.IsChecked == true)
            {
                var repo = AddFolder(root, "Repository");
                AddFolder(repo, "IRepository");
                AddFolder(repo, "Repository");
            }

            if (CqrsCheck.IsChecked == true)
            {
                var cqrs = AddFolder(root, "CQRS");
                AddFolder(cqrs, "Commands");
                AddFolder(cqrs, "Queries");
            }

            if (AutoMapperCheck.IsChecked == true)
            {
                AddFile(root, "AutoMapperProfiles");
            }

            root.IsExpanded = true;
        }

        // Método auxiliar para agregar carpeta al TreeView
        private TreeViewItem AddFolder(TreeViewItem parent, string name)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(name, "Folder.png"),
                IsExpanded = true
            };

            parent.Items.Add(item);
            return item;
        }

        // Método para agregar archivo al TreeView
        //private void AddFile(TreeViewItem parent, string name)
        //{
        //    parent.Items.Add(new TreeViewItem
        //    {
        //        Header = CreateHeader(name, "File.png")
        //    });
        //}

        private void AddFile(TreeViewItem parent, string fileName)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader($"{fileName}.cs", "File.png")
            };

            parent.Items.Add(item);
        }


        // Método auxiliar para crear un header con ícono y texto
        private StackPanel CreateHeader(string text, string iconRelativePath)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            string imagePath = Path.Combine(assemblyDirectory ?? "", "Resources", iconRelativePath);
            var uri = new Uri(imagePath, UriKind.Absolute);

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new Image
                    {
                        Source = new BitmapImage(uri),
                        Width = 16,
                        Height = 16,
                        Margin = new Thickness(0,0,5,0)
                    },
                    new TextBlock
                    {
                        Text = text,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }


    }
}
