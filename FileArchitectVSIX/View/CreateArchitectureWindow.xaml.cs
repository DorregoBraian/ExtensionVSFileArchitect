using EnvDTE;
using EnvDTE80;
using FileArchitectVSIX.Dtos;
using FileArchitectVSIX.Dtos.Enum;
using FileArchitectVSIX.IServices;
using FileArchitectVSIX.Services;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VSLangProj;
using Window = System.Windows.Window;

namespace FileArchitectVSIX
{
    /// <summary>
    /// Interaction logic for CreateArchitectureWindow.xaml
    /// </summary>
    public partial class CreateArchitectureWindow : Window
    {
        private readonly IArchitectureService _architectureService;
        public CreateArchitectureWindow()
        {
            InitializeComponent();
            _architectureService = new ArchitectureService(
                new FolderAndFileService(),
                new ProjectTemplateService()
            );
        }

        // Método para manejar el clic en el botón "Generar"
        private void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Validar arquitectura
            if (ArchitectureCombo.SelectedIndex <= 0)
            {
                MessageBox.Show("Seleccioná una arquitectura.");
                return;
            }

            // Obtener arquitectura seleccionada
            string architecture = ((ComboBoxItem)ArchitectureCombo.SelectedItem).Content.ToString();

            // Obtener namespace base
            string baseNameSpace = BaseNamespaceTextBox.Text?.Trim();

            // Validar namespace
            if (string.IsNullOrWhiteSpace(baseNameSpace))
            {
                MessageBox.Show("Ingresá el nombre base del proyecto / namespace.");
                return;
            }

            var request = new ArchitectureRequestDto
            {
                Architecture = (ArchitectureType)Enum.Parse(typeof(ArchitectureType), architecture),
                NameSpace = baseNameSpace,
                //ProjectName = null, //BaseNamespaceTextBox.Text.Trim(), // o pedir otro input
                Options = new ArchitectureOptionsDto
                {
                    UseRepository = RepositoryCheck.IsChecked == true,
                    UseCQRS = CqrsCheck.IsChecked == true,
                    UseUnitOfWork = UnitOfWorkCheck.IsChecked == true,
                    UseAutoMapper = AutoMapperCheck.IsChecked == true
                }
            };

            // Obtener el proyecto actual
            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));

            // Crear la arquitectura de carpetas
            var result = GenerateArchitecture(request, dte);

            MessageBox.Show(result.Message);
        }

        // Método para generar la arquitectura seleccionada
        private OperationResultDto GenerateArchitecture(ArchitectureRequestDto requestDto, DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            switch (requestDto.Architecture)
            {
                case ArchitectureType.Hexagonal:
                    return _architectureService.CreateHexagonalArchitecture(dte,requestDto);
                    //CreateWebApiProjectAndAddToSolution(dte, baseNameSpace);
                    //CreateHexagonalArchitecture(dte, baseNameSpace);

                //case ArchitectureType.Clean:
                    //CreateFolder(project, "Core");
                    //CreateFolder(project, "UseCases");
                    //CreateFolder(project, "InterfaceAdapters");
                    //CreateFolder(project, "Frameworks");
                    

                //case ArchitectureType.MVC:
                    //CreateFolder(project, "Models");
                    //CreateFolder(project, "Views");
                    //CreateFolder(project, "Controllers");
                    

                default:
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Seleccioná una arquitectura."
                    };
            }
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

        // Evento para actualizar la vista previa cuando cambia la selección
        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // Protección contra inicialización temprana
            if (BaseNamespaceLabel == null || BaseNamespaceTextBox == null)
                return;

            // Verificamos si eligió una arquitectura válida
            bool architectureSelected = ArchitectureCombo.SelectedIndex > 0;

            // Mostrar u ocultar label
            BaseNamespaceLabel.Visibility = architectureSelected
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Mostrar u ocultar textbox
            BaseNamespaceTextBox.Visibility = architectureSelected
                ? Visibility.Visible
                : Visibility.Collapsed;

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

            string architecture = ((ComboBoxItem)ArchitectureCombo.SelectedItem).Content.ToString();

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

        // Método auxiliar para agregar archivo al TreeView
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
