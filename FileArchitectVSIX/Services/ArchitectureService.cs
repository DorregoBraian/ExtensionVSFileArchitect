using EnvDTE;
using EnvDTE80;
using FileArchitectVSIX.Dtos;
using FileArchitectVSIX.IServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileArchitectVSIX.Services
{
    public class ArchitectureService : IArchitectureService
    {
        private readonly IFolderAndFileService _folderAndFileService;
        private readonly IProjectTemplateService _projectTemplateService;

        public ArchitectureService(IFolderAndFileService folderService, IProjectTemplateService projectTemplateService)
        {
            _folderAndFileService = folderService;
            _projectTemplateService = projectTemplateService;
        }

        // Método específico para arquitectura Hexagonal
        public OperationResultDto CreateHexagonalArchitecture(DTE2 dte, ArchitectureRequestDto request)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var option = new OperationResultDto();

            try
            {
                _projectTemplateService.CreateWebApiProjectAndAddToSolution(dte, request.ProjectName); // Crea el proyecto Web API principal

                var solution = (Solution2)dte.Solution; // Obtiene la solución actual

                if (string.IsNullOrWhiteSpace(request.NameSpace))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "BaseNamespace vacío"
                    };
                }

                // --------------- DOMAIN ---------------
                if (ProjectExists(solution, "Domain"))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "El proyecto 'Domain' ya existe en la solución."
                    };
                }

                var domain = _projectTemplateService.CreateClassLibraryProjectAndAddToSolution(dte, "Domain");
                _folderAndFileService.CreateFolder(domain, "Entities");
                _folderAndFileService.CreateFolder(domain, "IServices");

                // Repository (si se selecciona)
                if (request.Options.UseRepository)
                {
                    _folderAndFileService.CreateFolder(domain, "IRepository");
                }

                // --------------- APPLICATION ---------------
                if (ProjectExists(solution, "Application"))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "El proyecto 'Application' ya existe en la solución."
                    };
                }

                var application = _projectTemplateService.CreateClassLibraryProjectAndAddToSolution(dte, "Application");
                _folderAndFileService.CreateFolder(application, "DTOs");
                _folderAndFileService.CreateFolder(application, "IServices");

                // referencias entre proyectos
                _projectTemplateService.AddProjectReference(application, domain);

                // --------------- INFRASTRUCTURE ---------------
                if (ProjectExists(solution, "Infrastructure"))
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "El proyecto 'Infrastructure' ya existe en la solución."
                    };
                }

                var infrastructure = _projectTemplateService.CreateClassLibraryProjectAndAddToSolution(dte, "Infrastructure");
                _folderAndFileService.CreateDbContextFile(infrastructure, "DbContext");

                // Repository (si se selecciona)
                if (request.Options.UseRepository)
                {
                    _folderAndFileService.CreateFolder(infrastructure, "Repository");
                }

                // CQRS (solo si se selecciona)
                if (request.Options.UseCQRS)
                {
                    var commands = _folderAndFileService.CreateFolder(infrastructure, "Commands");
                    _folderAndFileService.CreateSubFolder(commands, "Create");
                    _folderAndFileService.CreateSubFolder(commands, "Update");
                    _folderAndFileService.CreateSubFolder(commands, "Delete");

                    var queries = _folderAndFileService.CreateFolder(infrastructure, "Queries");
                    _folderAndFileService.CreateSubFolder(queries, "Get");
                }

                // AutoMapper (si se selecciona)
                if (request.Options.UseAutoMapper)
                {
                    _folderAndFileService.CreateAutoMapperFile(infrastructure, "AutoMapperProfiles");
                }

                // Referencias entre proyectos
                _projectTemplateService.AddProjectReference(infrastructure, application);
                _projectTemplateService.AddProjectReference(infrastructure, domain);

                // Resultado exitoso
                return new OperationResultDto
                {
                    Success = true,
                    Message = "Arquitectura hexagonal creada correctamente."
                };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = "Error al crear la arquitectura hexagonal.",
                    Exception = ex
                };
            }
        }

        // Método para verificar si un proyecto ya existe en la solución
        private bool ProjectExists(Solution2 solution, string projectName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return solution.Projects
                .Cast<Project>()
                .Any(p => string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase));
        }


    }
}
