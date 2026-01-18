using EnvDTE;
using EnvDTE80;
using FileArchitectVSIX.Dtos;
using FileArchitectVSIX.IServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task<OperationResultDto> CreateHexagonalArchitectureAsync (DTE2 dte, ArchitectureRequestDto request, IProgress<ProgressReportDto> progress)
        {
            return await Task.Run(async () =>
            {
                var option = new OperationResultDto();

                try
                {
                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 5,
                        Message = "Inicializando arquitectura..."
                    });

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 15,
                        Message = "Creando proyecto API..."
                    });

                    // Crea el proyecto Web API principal
                    var apiProject = await _projectTemplateService.CreateWebApiProjectAndAddToSolutionAsync(dte, request.ProjectName);
                    await _folderAndFileService.CreateFolderAsync(apiProject, "Controller");

                    await Task.Yield();

                    var solution = (Solution2)dte.Solution; // Obtiene la solución actual

                    if (string.IsNullOrWhiteSpace(request.NameSpace))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "BaseNamespace vacío"
                        };
                    }

                    // ------------------------------ DOMAIN ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 30,
                        Message = "Creando proyecto Domain..."
                    });

                    if (ProjectExists(solution, "Domain"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Domain' ya existe en la solución."
                        };
                    }

                    var domain = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Domain");
                    await _folderAndFileService.CreateFolderAsync(domain, "Entities");

                    // Repository (si se selecciona)
                    if (request.Options.UseRepository)
                    {
                        await _folderAndFileService.CreateFolderAsync(domain, "IRepository");
                    }

                    await Task.Yield();

                    // ------------------------------ APPLICATION ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    
                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 50,
                        Message = "Creando proyecto Application..."
                    });

                    if (ProjectExists(solution, "Application"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Application' ya existe en la solución."
                        };
                    }

                    var application = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Application");
                    await _folderAndFileService.CreateFolderAsync(application, "DTOs");
                    await _folderAndFileService.CreateFolderAsync(application, "IServices");
                    await _folderAndFileService.CreateFolderAsync(application, "Services");

                    // AutoMapper (si se selecciona)
                    if (request.Options.UseAutoMapper)
                    {
                        await _folderAndFileService.CreateAutoMapperFileAsync(application, "AutoMapperProfiles");
                    }

                    // CQRS (solo si se selecciona)
                    if (request.Options.UseCQRS)
                    {
                        var commands = await _folderAndFileService.CreateFolderAsync(application, "Commands");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Create");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Update");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Delete");

                        var queries = await _folderAndFileService.CreateFolderAsync(application, "Queries");
                        await _folderAndFileService.CreateSubFolderAsync(queries, "Get");
                    }

                    // referencias entre proyectos
                    await _projectTemplateService.AddProjectReferenceAsync(application, domain);

                    await Task.Yield();

                    // ------------------------------ INFRASTRUCTURE ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 70,
                        Message = "Creando proyecto Infrastructure..."
                    });

                    if (ProjectExists(solution, "Infrastructure"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Infrastructure' ya existe en la solución."
                        };
                    }

                    var infrastructure = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Infrastructure");
                    await _folderAndFileService.CreateDbContextFileAsync(infrastructure, "DbContext");

                    // Repository (si se selecciona)
                    if (request.Options.UseRepository)
                    {
                        await _folderAndFileService.CreateFolderAsync(infrastructure, "IRepository");
                    }

                    // Referencias entre proyectos
                    await _projectTemplateService.AddProjectReferenceAsync(infrastructure, application);
                    await _projectTemplateService.AddProjectReferenceAsync(infrastructure, domain);

                    await Task.Yield();

                    // ------------------------------ TEST ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    
                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 85,
                        Message = "Creando proyecto Test..."
                    });

                    if (request.Options.UserTestingProject)
                    {

                        var testProject = await _projectTemplateService.CreateTestProjectAndToSolutionAsync(dte, "Tests");

                        // Creo las carpetas de prueba
                        await _folderAndFileService.CreateFolderAsync(testProject, "ControllerTests");
                        await _folderAndFileService.CreateFolderAsync(testProject, "ServiceTests");
                        await _folderAndFileService.CreateFolderAsync(testProject, "RepositoryTests");
                    }
                
                    // ------------------------------ API PROJECT REFERENCIAS ------------------------------
                    
                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 100,
                        Message = "Agregando referencias entre proyectos..."
                    });

                    await _projectTemplateService.AddProjectReferenceAsync(apiProject, application);
                    await _projectTemplateService.AddProjectReferenceAsync(apiProject, infrastructure);

                    await Task.Yield();

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
            });

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
