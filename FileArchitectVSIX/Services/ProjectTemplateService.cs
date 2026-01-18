using EnvDTE;
using EnvDTE80;
using FileArchitectVSIX.IServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
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

            // Buscar y retornar el proyecto recién creado
            var proj = dte.Solution.Projects.Cast<Project>().First(p => p.Name == projectName);
            return proj;
        }

        // Método para crear un proyecto Web API y agregarlo a la solución
        public async Task<Project> CreateWebApiProjectAndAddToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta física donde está la solución (.sln)
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

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

            // Buscar y retornar el proyecto recién creado
            var proj = dte.Solution.Projects.Cast<Project>().First(p => p.Name == projectName);
            return proj;
        }

        // Método para crear un proyecto de pruebas xUnit y agregarlo a la solución
        public async Task<Project> CreateTestProjectAndToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta donde vive la solución (.sln)
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

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

            // Buscar y retornar el proyecto recién creado
            var proj = dte.Solution.Projects.Cast<Project>().First(p => p.Name == projectName);
            return proj;
        }

        // Método para crear un proyecto MVC y agregarlo a la solución
        public async Task CreateMvcProjectAndAddToSolutionAsync (DTE2 dte, string projectName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Ruta donde vive la solución (.sln)
            string solutionPath = Path.GetDirectoryName(dte.Solution.FullName);

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

            // Agregar el proyecto a la solución
            dte.Solution.AddFromFile(projectFile);
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


    }
}
