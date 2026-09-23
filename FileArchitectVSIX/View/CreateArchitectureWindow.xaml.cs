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
using System.Threading.Tasks;
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

        // Método para generar la arquitectura seleccionada
        private async Task<OperationResultDto> GenerateArchitectureAsync (ArchitectureRequestDto requestDto, DTE2 dte, IProgress<ProgressReportDto> progress)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            switch (requestDto.Architecture)
            {
                case ArchitectureType.Hexagonal:
                    return await _architectureService.CreateHexagonalArchitectureAsync (dte, requestDto, progress);
                
                case ArchitectureType.MVC:
                    return await _architectureService.CreateMvcArchitectureAsync (dte, requestDto, progress);

                case ArchitectureType.Clean:
                    return await _architectureService.CreateCleanArchitectureAsync (dte, requestDto, progress);

                default:
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Seleccioná una arquitectura."
                    };
            }
        }
        
        // Método para el botón "Generar"
        private async void OnGenerateClickedAsync (object sender, RoutedEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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
                ProjectName = baseNameSpace, // Nombre base para los proyectos web API y test
                Options = new ArchitectureOptionsDto
                {
                    UseRepository = RepositoryCheck.IsChecked == true,
                    UseCQRS = CqrsCheck.IsChecked == true,
                    //UseUnitOfWork = UnitOfWorkCheck.IsChecked == true,
                    UseAutoMapper = AutoMapperCheck.IsChecked == true,
                    UserTestingProject = TestCheck.IsChecked == true,
                    UserSqlServer = SqlServerCheck.IsChecked == true,
                    UserPostgreSQL = PostgreSQLCheck.IsChecked == true,
                    UserMongoDB = MongoDBCheck.IsChecked == true
                }
            };

            // Obtener el proyecto actual
            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));

            var progress = new Progress<ProgressReportDto>(report =>
            {
                _ = UpdateProgressAsync(report);
            });

            await ShowLoadingAsync();

            // Crear la arquitectura de carpetas
            var result = await GenerateArchitectureAsync (request, dte, progress);

            await HideLoadingAsync();

            MessageBox.Show(
                result.Message,
                "Generador de Arquitectura",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Error
            );

            if (result.Success)
            {
                this.Close();
            }
        }

        // Método para el boton "Resetear"
        private async void OnResetClickedAsync (object sender, RoutedEventArgs e)
        {
            // Reset arquitectura
            ArchitectureCombo.SelectedIndex = 0;

            // Reset texto
            BaseNamespaceTextBox.Text = string.Empty;

            // Reset patrones
            RepositoryCheck.IsChecked = false;
            CqrsCheck.IsChecked = false;
            //UnitOfWorkCheck.IsChecked = false;
            AutoMapperCheck.IsChecked = false;
            TestCheck.IsChecked = false;
            SqlServerCheck.IsChecked = false;
            PostgreSQLCheck.IsChecked = false;
            MongoDBCheck.IsChecked = false;

            // Ocultar controles dependientes
            BaseNamespaceLabel.Visibility = Visibility.Collapsed;
            BaseNamespaceContainer.Visibility = Visibility.Collapsed;
            PatternsPanel.Visibility = Visibility.Collapsed;
            ActionsPanel.Visibility = Visibility.Collapsed;
            DatabasePanel.Visibility = Visibility.Collapsed;

            // Limpiar preview
            PreviewTree.Items.Clear();
        }

        // Evento para actualizar la vista previa cuando cambia la selección
        private async void OnSelectionChangedAsync (object sender, RoutedEventArgs e)
        {
            // protección contra inicialización temprana
            if (BaseNamespaceLabel == null || BaseNamespaceTextBox == null || PatternsPanel == null || ActionsPanel == null)
                return;

            bool architectureSelected = ArchitectureCombo.SelectedIndex > 0;

            // mostrar solo la etiqueta y el TextBox del nombre
            // Mostrar input de nombre
            BaseNamespaceLabel.Visibility = Visibility.Visible;
            BaseNamespaceContainer.Visibility = Visibility.Visible;

            // Resetear estado inicial
            BaseNamespaceTextBox.Text = string.Empty;
            BaseNamespacePlaceholder.Visibility = Visibility.Visible;

            // esconder patrones y botones hasta que se escriba el nombre
            PatternsPanel.Visibility = Visibility.Collapsed;
            ActionsPanel.Visibility = Visibility.Collapsed;
            DatabasePanel.Visibility = Visibility.Collapsed;

            // limpiar preview si se deseleccionó
            await UpdatePreviewAsync ();
        }

        // Evento para actualizar la vista previa cuando cambia el texto del namespace base
        private async void OnBaseNamespaceTextChangedAsync (object sender, TextChangedEventArgs e)
        {
            if (BaseNamespaceTextBox == null || BaseNamespacePlaceholder == null || PatternsPanel == null || ActionsPanel == null || DatabasePanel == null)
                return;

            string txt = BaseNamespaceTextBox.Text?.Trim();

            bool hasName = !string.IsNullOrWhiteSpace(txt);

            BaseNamespacePlaceholder.Visibility =string.IsNullOrWhiteSpace(txt) ? Visibility.Visible : Visibility.Collapsed;
            PatternsPanel.Visibility = hasName ? Visibility.Visible : Visibility.Collapsed;
            ActionsPanel.Visibility = hasName ? Visibility.Visible : Visibility.Collapsed;
            DatabasePanel.Visibility = hasName ? Visibility.Visible : Visibility.Collapsed;

            // actualizamos la preview con el nombre (para que muestre el nombre de la solución/proyecto)
            await UpdatePreviewAsync();
        }

        // Evento para actualizar la vista previa cuando cambia algún patrón
        private async void OnPatternChangedAsync (object sender, RoutedEventArgs e)
        {
            await UpdatePreviewAsync();
        }

        // Método para actualizar la vista previa del TreeView
        private async Task UpdatePreviewAsync()
        {
            PreviewTree.Items.Clear();

            // Sin arquitectura → no mostrar nada
            if (ArchitectureCombo.SelectedIndex <= 0)
                return;

            string baseName = string.IsNullOrWhiteSpace(BaseNamespaceTextBox.Text)
                ? "Project"
                : BaseNamespaceTextBox.Text.Trim();

            // ROOT
            var root = new TreeViewItem
            {
                Header = CreateHeader(baseName, "IconProject.png"),
                IsExpanded = true
            };

            PreviewTree.Items.Add(root);

            TreeViewItem api = null;
            TreeViewItem domain = null;
            TreeViewItem application = null;
            TreeViewItem infrastructure = null;
            TreeViewItem test = null;

            switch (ArchitectureCombo.SelectedIndex)
            {
                case 1: // HEXAGONAL
                    BuildHexagonalArchitecture (root, baseName, out api, out domain, out application, out infrastructure, out test);
                    break;

                case 2: // CLEAN
                    BuildCleanArchitecture (root, baseName, out api, out domain, out application, out infrastructure, out test);
                    break;

                case 3: // MVC
                    BuildMVCArchitecture (root, baseName, out api, out test);
                    break;

                default:
                    break;
            }

            // SOLO si el usuario marcó patrones
            await ApplyPatternsInTreeViewAsync (api, domain, application, infrastructure, test);

            // Expandir nodos
            if (api != null)
                api.IsExpanded = true;

            if (domain != null)
                domain.IsExpanded = true;

            if (application != null)
                application.IsExpanded = true;

            if (infrastructure != null)
                infrastructure.IsExpanded = true;

            if (test != null)
                test.IsExpanded = true;

        }

        // Método para aplicar patrones seleccionados al TreeView
        private async Task ApplyPatternsInTreeViewAsync (TreeViewItem api, TreeViewItem domain, TreeViewItem application, TreeViewItem infrastructure, TreeViewItem test)
        {
            switch(ArchitectureCombo.SelectedIndex)
                {
                case 1: // HEXAGONAL
                    // Repository
                    if (RepositoryCheck.IsChecked == true)
                    {
                        AddFolder(domain, "IRepository");
                        AddFolder(infrastructure, "Repository");
                    }

                    // CQRS
                    if (CqrsCheck.IsChecked == true)
                    {
                        var commands = AddFolder(application, "Commands");
                        AddFolder(commands, "Create");
                        AddFolder(commands, "Update");
                        AddFolder(commands, "Delete");

                        var queries = AddFolder(application, "Queries");
                        AddFolder(queries, "Get");
                    }

                    // Unit of Work
                    //if (UnitOfWorkCheck.IsChecked == true)
                    //{
                    //    AddFolder(infrastructure, "UnitOfWork");
                    //}

                    // AutoMapper
                    if (AutoMapperCheck.IsChecked == true)
                    {
                        AddFile(application, "AutoMapperProfiles");
                    }

                    // Testing
                    if (TestCheck.IsChecked == true)
                    {
                        AddFolder(test, "ControllerTest");
                        AddFolder(test, "ServiceTest");
                        AddFolder(test, "RepositoryTest");
                    }

                    break;
                case 2: // CLEAN
                    // Repository
                    if (RepositoryCheck.IsChecked == true)
                    {
                        AddFolder(application, "IRepository");
                        AddFolder(infrastructure, "Repository");
                    }

                    // CQRS
                    if (CqrsCheck.IsChecked == true)
                    {
                        var commands = AddFolder(application, "Commands");
                        AddFolder(commands, "Create");
                        AddFolder(commands, "Update");
                        AddFolder(commands, "Delete");

                        var queries = AddFolder(application, "Queries");
                        AddFolder(queries, "Get");
                    }

                    // Unit of Work
                    //if (UnitOfWorkCheck.IsChecked == true)
                    //{
                    //    AddFolder(infrastructure, "UnitOfWork");
                    //}

                    // AutoMapper
                    if (AutoMapperCheck.IsChecked == true)
                    {
                        AddFile(application, "AutoMapperProfiles");
                    }

                    // Testing
                    if (TestCheck.IsChecked == true)
                    {
                        AddFolder(test, "ControllerTest");
                        AddFolder(test, "ServiceTest");
                        AddFolder(test, "RepositoryTest");
                    }
                    break;
                case 3: // MVC
                    // Repository
                    var data = FindFolder(api, "Data");
                    if (data != null && RepositoryCheck.IsChecked == true)
                    {
                        AddFolder(data, "IRepository");
                        AddFolder(data, "Repository");
                    }

                    // CQRS
                    var services = FindFolder(api, "Services");
                    if (services != null && CqrsCheck.IsChecked == true)
                    {
                        var commands = AddFolder(services, "Commands");
                        AddFolder(commands, "Create");
                        AddFolder(commands, "Update");
                        AddFolder(commands, "Delete");
                        var queries = AddFolder(services, "Queries");
                        AddFolder(queries, "Get");
                    }

                    // Unit of Work
                    //if (services != null && UnitOfWorkCheck.IsChecked == true)
                    //{
                    //    AddFolder(services, "UnitOfWork");
                    //}

                    // AutoMapper
                    var Helpers = FindFolder(api, "Helpers");
                    if (Helpers != null && AutoMapperCheck.IsChecked == true)
                    {
                        AddFile(Helpers, "AutoMapperProfiles");
                    }

                    // Testing
                    if (TestCheck.IsChecked == true)
                    {
                        AddFolder(test, "ControllerTest");
                        AddFolder(test, "ServiceTest");
                        AddFolder(test, "RepositoryTest");
                    }
                    break;
                default:
                    break;
            }
        }

        private void BuildHexagonalArchitecture (TreeViewItem root, string baseName, out TreeViewItem api, out TreeViewItem domain, out TreeViewItem application, out TreeViewItem infrastructure, out TreeViewItem test)
        {
            api = CreateProject(root, baseName);
            AddFolder(api, "Controllers");

            domain = CreateProject(root, "Domain");
            AddFolder(domain, "Entities");

            application = CreateProject(root, "Application");
            var dtos = AddFolder(application, "DTOs");
            AddFolder(dtos, "Requests");
            AddFolder(dtos, "Responses");
            AddFolder(application, "Services");
            AddFolder(application, "IServices");

            infrastructure = CreateProject(root, "Infrastructure");
            AddFile(infrastructure, "DbContext.cs");

            test = CreateProject(root, "Tests");
        }

        private void BuildCleanArchitecture (TreeViewItem root, string baseName, out TreeViewItem api, out TreeViewItem domain, out TreeViewItem application, out TreeViewItem infrastructure, out TreeViewItem test)
        {
            api = CreateProject(root, baseName);
            AddFolder(api, "Controllers");

            domain = CreateProject(root, "Domain");
            AddFolder(domain, "Entities");

            application = CreateProject(root, "Application");
            var dtos = AddFolder(application, "DTOs");
            AddFolder(dtos, "Requests");
            AddFolder(dtos, "Responses");
            AddFolder(application, "IServices");

            infrastructure = CreateProject(root, "Infrastructure");
            AddFolder(infrastructure, "Services");
            AddFile(infrastructure, "DbContext.cs");

            test = CreateProject(root, "Tests");
        }

        private void BuildMVCArchitecture (TreeViewItem root, string baseName, out TreeViewItem api, out TreeViewItem test)
        {
            api = CreateProject(root, baseName);
            AddFolder(api, "Controllers");

            var modals = AddFolder(api, "Models");
            AddFolder(modals, "ViewModels");
            AddFolder(modals, "Entitie");

            var dto = AddFolder(modals, "DTOs");
            AddFolder(dto, "Requests");
            AddFolder(dto, "Responses");

            AddFolder(api, "Views");

            var services = AddFolder(api, "Services");
            AddFolder(services, "IServices");
            AddFolder(services, "Services");

            var data = AddFolder(api, "Data");
            AddFolder(data, "DbContexts");

            var helpers =AddFolder(api, "Helpers");

            AddFolder(api, "ViewComponents");

            test = CreateProject(root, "Tests");
        }

        // ---------------------- Métodos auxiliares para el TreeView ----------------------

        // Método auxiliar para crear un proyecto en el TreeView
        private TreeViewItem CreateProject (TreeViewItem root, string foldeName)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(foldeName, "IconProject.png"),
                Tag = foldeName,
                IsExpanded = true
            };

            root.Items.Add(item);
            return item;
        }

        // Método auxiliar para agregar carpeta al TreeView
        private TreeViewItem AddFolder (TreeViewItem parent, string foldeName)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(foldeName, "folder.png"),
                Tag = foldeName,
                IsExpanded = true
            };

            parent.Items.Add(item);
            return item;
        }

        // Método auxiliar para agregar archivo al TreeView
        private void AddFile (TreeViewItem parent, string fileName)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader($"{fileName}{(fileName.EndsWith(".cs") ? "" : ".cs")}", "file.png"),
                Tag = fileName
            };

            parent.Items.Add(item);
        }

        // Metodo para buscar carpetas ya existentes en el TreeView
        private TreeViewItem FindFolder (TreeViewItem parent, string folderName)
        {
            foreach (TreeViewItem item in parent.Items)
            {
                if (item.Tag?.ToString() == folderName)
                    return item;
            }

            return null;
        }

        // Método auxiliar para crear un header con ícono y texto
        private StackPanel CreateHeader (string text, string iconRelativePath)
        {
            // buscamos la imagen en la carpeta del ensamblado (output)
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? "";
            string imagePath = Path.Combine(assemblyDirectory, "Resources", iconRelativePath);

            // protecciones: si no existe el archivo, no rompas la UI
            Image img = new Image { Width = 16, Height = 16, Margin = new Thickness(0, 0, 6, 0) };

            try
            {
                if (File.Exists(imagePath))
                {
                    img.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                }
            }
            catch
            {
                // ignorar error de carga
            }

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(img);
            panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });

            return panel;
        }

        // Método para mostrar el overlay de carga
        private async Task ShowLoadingAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            MainContent.IsEnabled = false;
            LoadingOverlay.Visibility = Visibility.Visible;

            await Task.Yield();
        }

        // Método para ocultar el overlay de carga
        private async Task HideLoadingAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            LoadingOverlay.Visibility = Visibility.Collapsed;
            MainContent.IsEnabled = true;
        }

        // Método para actualizar la barra de progreso
        private async Task UpdateProgressAsync(ProgressReportDto report)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ProgressBar.Value = report.Percentage;
            ProgressText.Text = report.Message;
        }
    }
}
