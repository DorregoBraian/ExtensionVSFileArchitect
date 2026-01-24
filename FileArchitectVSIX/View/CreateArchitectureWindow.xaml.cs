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

                case ArchitectureType.Clean:
                    //return await _architectureService.CreateCleanArchitectureAsync(dte, requestDto, progress);

                case ArchitectureType.MVC:
                    //return await _architectureService.CreateMvcArchitectureAsync(dte, requestDto, progress);

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
                    UseUnitOfWork = UnitOfWorkCheck.IsChecked == true,
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
            UnitOfWorkCheck.IsChecked = false;
            AutoMapperCheck.IsChecked = false;
            TestCheck.IsChecked = false;
            SqlServerCheck.IsChecked = false;
            PostgreSQLCheck.IsChecked = false;
            MongoDBCheck.IsChecked = false;

            // Ocultar controles dependientes
            BaseNamespaceLabel.Visibility = Visibility.Collapsed;
            BaseNamespaceTextBox.Visibility = Visibility.Collapsed;
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

            // SOLO proyectos base (SIEMPRE)
            var api = CreateProject(root, $"{baseName}");
            var domain = CreateProject(root, $"Domain");
            var application = CreateProject(root, $"Application");
            var infrastructure = CreateProject(root, $"Infrastructure");
            var test = CreateProject(root, $"Tests");

            // Carpetas mínimas base
            AddFolder (api, "Controllers");
            AddFolder(domain, "Entities");
            AddFolder(application, "DTOs");
            AddFolder(application, "Services");
            AddFolder(application, "IServices");
            AddFile(infrastructure, "DbContext.cs");

            // SOLO si el usuario marcó patrones
            await ApplyPatternsInTreeViewAsync (domain, application, infrastructure, test);

            // Expandir nodos
            api.IsExpanded = true;
            domain.IsExpanded = true;
            application.IsExpanded = true;
            infrastructure.IsExpanded = true;
            test.IsExpanded = true;
        }

        // Método para aplicar patrones seleccionados al TreeView
        private async Task ApplyPatternsInTreeViewAsync (TreeViewItem domain, TreeViewItem application, TreeViewItem infrastructure, TreeViewItem test)
        {
            // Repository
            if (RepositoryCheck.IsChecked == true)
            {
                AddFolder (domain, "IRepository");
                AddFolder (infrastructure, "Repository");
            }

            // CQRS
            if (CqrsCheck.IsChecked == true)
            {
                var commands = AddFolder (application, "Commands");
                AddFolder (commands, "Create");
                AddFolder (commands, "Update");
                AddFolder (commands, "Delete");

                var queries = AddFolder(application, "Queries");
                AddFolder (queries, "Get");
            }

            // Unit of Work
            if (UnitOfWorkCheck.IsChecked == true)
            {
                AddFolder (infrastructure, "UnitOfWork");
            }

            // AutoMapper
            if (AutoMapperCheck.IsChecked == true)
            {
                AddFile (application, "AutoMapperProfiles");
            }

            if (TestCheck.IsChecked == true)
            {
                AddFolder (test, "ControllerTest");
                AddFolder (test, "ServiceTest");
                AddFolder (test, "RepositoryTest");
            }
        }

        // ---------------------- Métodos auxiliares para el TreeView ----------------------

        // Método auxiliar para crear un proyecto en el TreeView
        private TreeViewItem CreateProject (TreeViewItem root, string foldeName)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(foldeName, "IconProject.png"),
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
                Header = CreateHeader($"{fileName}{(fileName.EndsWith(".cs") ? "" : ".cs")}", "file.png")
            };

            parent.Items.Add(item);
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
