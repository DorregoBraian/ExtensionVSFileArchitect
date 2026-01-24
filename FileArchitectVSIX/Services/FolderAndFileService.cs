using EnvDTE;
using FileArchitectVSIX.IServices;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Threading.Tasks;

namespace FileArchitectVSIX.Services
{
    public class FolderAndFileService : IFolderAndFileService
    {

        // Método para crear una carpeta en el proyecto
        public async Task<ProjectItem> CreateFolderAsync(Project project, string folderName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string projectPath = Path.GetDirectoryName(project.FullName); // Ruta física del proyecto
            string folderPath = Path.Combine(projectPath, folderName); // Ruta completa de la nueva carpeta

            // Crear carpeta en disco si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Agregarla al proyecto de Visual Studio
            return project.ProjectItems.AddFromDirectory(folderPath);
        }

        // Método para crear una subcarpeta dentro de una carpeta existente en el proyecto
        public async Task<ProjectItem> CreateSubFolderAsync(ProjectItem parentFolder, string folderName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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
        public async Task CreateAutoMapperFileAsync(Project project, string fileName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string folderPath = Path.GetDirectoryName(project.FileName);
            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            // Crear archivo si no existe
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $@"
                using AutoMapper;

                namespace {project.Name}
                {{
                    public class AutoMapperProfile : Profile
                    {{
                        public AutoMapperProfile()
                        {{
                            // Configurá tus mapeos aquí, Por ejemplo:
                            // CreateMap<User, UserDto>()
                        }}

                    }}
                }}
                ".Trim());
            }

            // Agregar al proyecto
            project.ProjectItems.AddFromFile(filePath);
        }

        // Método para crear un archivo DbContext
        public async Task CreateDbContextFileAsync(Project project, string contextName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            string folderPath = Path.GetDirectoryName(project.FileName);
            string filePath = Path.Combine(folderPath, "DbContext.cs");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $@"
                using Microsoft.EntityFrameworkCore;

                namespace {project.Name}
                {{
                    public class {contextName}DbContext : DbContext
                    {{
                        // DbSets
                        // public DbSet<YourEntity> YourEntities {{ get; set; }}
                        
                        // Constructor      
                        public {contextName}DbContext(DbContextOptions<{contextName}DbContext> options)
                            : base(options)
                        {{
                        }}
                        
                        // Configurá el modelo aquí
                        protected override void OnModelCreating(ModelBuilder modelBuilder)
                        {{
                            base.OnModelCreating(modelBuilder);
                        }}
                    }}
                }}
                ".Trim());
            }

            // Agregar al proyecto
            project.ProjectItems.AddFromFile(filePath);
        }

        



    }
}
