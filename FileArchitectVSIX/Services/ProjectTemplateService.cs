using EnvDTE;
using EnvDTE80;
using FileArchitectVSIX.IServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSLangProj;

namespace FileArchitectVSIX.Services
{
    public class ProjectTemplateService : IProjectTemplateService
    {
        // Método para crear un proyecto de biblioteca de clases y agregarlo a la solución
        public async Task<Project> CreateClassLibraryProjectAndAddToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta física de la solución
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

            if (string.IsNullOrEmpty(solutionPath))
                throw new InvalidOperationException("La solución no está guardada. Guárdala primero.");

            // Ejecuta: dotnet new classlib -n {projectName}
            var process = new System.Diagnostics.Process();  // Proceso para ejecutar dotnet CLI
            process.StartInfo.FileName = "dotnet";           // Comando dotnet
            process.StartInfo.Arguments = $"new classlib -n {projectName}"; // Argumentos para crear classlib
            process.StartInfo.WorkingDirectory = solutionPath; // Directorio de trabajo: ruta de la solución
            process.StartInfo.CreateNoWindow = true;          // No mostrar ventana de consola
            process.StartInfo.UseShellExecute = false;      // No usar shell para ejecutar

            process.Start();
            process.WaitForExit();

            // Ruta del .csproj generado
            string projectFile = Path.Combine(
                solutionPath,
                projectName,
                $"{projectName}.csproj"
            );

            // Agregar el proyecto a la solución
            dte.Solution.AddFromFile(projectFile);

            Project proj = null;
            for (int i = 0; i < 10; i++)
            {
                // Buscar y retornar el proyecto recién creado
                proj = dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == projectName);

                if (proj != null) break;

                await Task.Delay(200);
            }
            if (proj == null)
                throw new Exception("No se pudo encontrar el proyecto después de agregarlo.");

            return proj;
        }

        // Método para crear un proyecto Web API y agregarlo a la solución
        public async Task<Project> CreateWebApiProjectAndAddToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta física donde está la solución (.sln)
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

            if (string.IsNullOrEmpty(solutionPath))
                throw new InvalidOperationException("La solución no está guardada. Guárdala primero.");

            // Ejecuta: dotnet new webapi -n {projectName}
            var process = new System.Diagnostics.Process();  // Proceso para ejecutar dotnet CLI
            process.StartInfo.FileName = "dotnet";          // Comando dotnet
            process.StartInfo.Arguments = $"new webapi -n {projectName}"; // Argumentos para crear webapi
            process.StartInfo.WorkingDirectory = solutionPath; // Directorio de trabajo: ruta de la solución
            process.StartInfo.CreateNoWindow = true;         // No mostrar ventana de consola
            process.StartInfo.UseShellExecute = false;     // No usar shell para ejecutar

            process.Start();
            process.WaitForExit();

            // Ruta del archivo .csproj recién creado
            string projectFile = Path.Combine(
                solutionPath,
                projectName,
                $"{projectName}.csproj"
            );

            // Agregar el proyecto a la solución
            dte.Solution.AddFromFile(projectFile);

            Project proj = null;
            for (int i = 0; i < 10; i++)
            {
                // Buscar y retornar el proyecto recién creado
                proj = dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == projectName);

                if (proj != null) break;

                await Task.Delay(200);
            }
            if (proj == null)
                throw new Exception("No se pudo encontrar el proyecto después de agregarlo.");

            return proj;
        }

        // Método para crear un proyecto de pruebas xUnit y agregarlo a la solución
        public async Task<Project> CreateTestProjectAndToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta donde vive la solución (.sln)
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

            if (string.IsNullOrEmpty(solutionPath))
                throw new InvalidOperationException("La solución no está guardada. Guárdala primero.");

            // Ejecuta: dotnet new xunit -n {projectName}
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"new xunit -n {projectName}";
            process.StartInfo.WorkingDirectory = solutionPath;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();

            // Ruta del proyecto generado
            string projectFile = Path.Combine(
                solutionPath,
                projectName,
                $"{projectName}.csproj"
            );

            // Agregar el proyecto a la solución
            dte.Solution.AddFromFile(projectFile);

            Project proj = null;
            for (int i = 0; i < 10; i++)
            {
                // Buscar y retornar el proyecto recién creado
                proj = dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == projectName);

                if (proj != null) break;

                await Task.Delay(200);
            }
            if (proj == null)
                throw new Exception("No se pudo encontrar el proyecto después de agregarlo.");

            return proj;
        }

        // Método para crear un proyecto MVC y agregarlo a la solución
        public async Task<Project> CreateMvcProjectAndAddToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta donde vive la solución (.sln)
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

            if (string.IsNullOrEmpty(solutionPath))
                throw new InvalidOperationException("La solución no está guardada. Guárdala primero.");

            // Ejecuta: dotnet new mvc -n {projectName}
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"new mvc -n {projectName}";
            process.StartInfo.WorkingDirectory = solutionPath;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            process.Start();
            process.WaitForExit();

            // Ruta del proyecto generado
            string projectFile = Path.Combine(
                solutionPath,
                projectName,
                $"{projectName}.csproj"
            );

            if (!File.Exists(projectFile))
                            throw new Exception($"No se encontró el archivo de proyecto: {projectFile}");

            // Agregar el proyecto a la solución
            dte.Solution.AddFromFile(projectFile);

            Project proj = null;
            for (int i = 0; i < 10; i++)
            {
                // Buscar y retornar el proyecto recién creado
                proj = dte.Solution.Projects.Cast<Project>().FirstOrDefault(p => p.Name == projectName);

                if (proj != null) break;

                await Task.Delay(200);
            }
            if (proj == null)
                throw new Exception("No se pudo encontrar el proyecto después de agregarlo.");

            return proj;
        }

        // Método para agregar una referencia de proyecto
        public async Task AddProjectReferenceAsync (Project from, Project to)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var vsProject = from.Object as VSProject;

            if (vsProject == null)
                return;

            vsProject.References.AddProject(to);
        }

        // Método para instalar un paquete NuGet en un proyecto
        public async Task AddNuGetPackageAsync(Project project, string packageId, string version = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

            var installer = componentModel.GetService<IVsPackageInstaller>();

            if (installer == null)
                throw new InvalidOperationException("No se pudo obtener IVsPackageInstaller");

            installer.InstallPackage(
                source: null,                 
                project: project,
                packageId: packageId,
                version: version,
                ignoreDependencies: false
            );
        }


    }
}
