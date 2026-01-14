using EnvDTE;
using FileArchitectVSIX.IServices;
using Microsoft.VisualStudio.Shell;
using System.IO;

namespace FileArchitectVSIX.Services
{
    public class FolderAndFileService : IFolderAndFileService
    {
      
        // Método para crear una carpeta en el proyecto
        public ProjectItem CreateFolder(Project project, string folderName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
        public ProjectItem CreateSubFolder(ProjectItem parentFolder, string folderName)
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
        public void CreateAutoMapperFile(Project project, string fileName)
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

        // Método para crear un archivo DbContext
        public void CreateDbContextFile(Project project, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string folderPath = Path.GetDirectoryName(project.FileName);
            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            // Crear archivo si no existe
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $@"namespace {project.Name}
                {{
                    public class {fileName}DbContext : DbContext
                    {{
                        // Definí tus DbSets aquí
                        // public DbSet<YourEntity> YourEntities {{ get; set; }}


                        // Constructor

                        public {fileName}DbContext (DbContextOptions<{fileName}DbContext> options) : base(options)
                        {{

                        }}

                        // Configurá el modelo aquí

                        protected override void OnModelCreating(ModelBuilder modelBuilder)
                        {{

                        }}
                    }}
                }}");
            }

            // Agregar al proyecto
            project.ProjectItems.AddFromFile(filePath);
        }




    }
}
