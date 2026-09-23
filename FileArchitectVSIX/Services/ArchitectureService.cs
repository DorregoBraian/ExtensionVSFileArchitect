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
                    
                    var solution = (Solution2)dte.Solution; // Obtiene la solución actual

                    if (solution == null)
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "No hay una solución abierta."
                        };
                    }

                    if (ProjectExists(solution, request.ProjectName))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = $"El proyecto '{request.ProjectName}' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 25,
                        Message = "Creando proyecto API..."
                    });

                    // Crea el proyecto Web API principal
                    var apiProject = await _projectTemplateService.CreateWebApiProjectAndAddToSolutionAsync(dte, request.ProjectName);
                    await _folderAndFileService.CreateFolderAsync(apiProject, "Controller");

                    if (string.IsNullOrWhiteSpace(request.NameSpace))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "BaseNamespace vacío"
                        };
                    }
                    
                    await Task.Yield();

                    // ------------------------------ DOMAIN ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    
                    if (ProjectExists(solution, "Domain"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Domain' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 45,
                        Message = "Creando proyecto Domain..."
                    });

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
                    
                    if (ProjectExists(solution, "Application"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Application' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 65,
                        Message = "Creando proyecto Application..."
                    });

                    var application = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Application");
                    var dtosFolder = await _folderAndFileService.CreateFolderAsync(application, "DTOs");
                    await _folderAndFileService.CreateSubFolderAsync(dtosFolder, "Responses");
                    await _folderAndFileService.CreateSubFolderAsync(dtosFolder, "Requests");
                    await _folderAndFileService.CreateFolderAsync(application, "IServices");
                    await _folderAndFileService.CreateFolderAsync(application, "Services");

                    // AutoMapper
                    if (request.Options.UseAutoMapper)
                    {
                        await _folderAndFileService.CreateAutoMapperFileAsync(application, "AutoMapperProfiles");
                    }

                    // CQRS
                    if (request.Options.UseCQRS)
                    {
                        var commands = await _folderAndFileService.CreateFolderAsync(application, "Commands");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Create");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Update");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Delete");

                        var queries = await _folderAndFileService.CreateFolderAsync(application, "Queries");
                        await _folderAndFileService.CreateSubFolderAsync(queries, "Get");
                    }

                    await Task.Yield();

                    // ------------------------------ INFRASTRUCTURE ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    
                    if (ProjectExists(solution, "Infrastructure"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Infrastructure' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 85,
                        Message = "Creando proyecto Infrastructure..."
                    });

                    var infrastructure = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Infrastructure");
                    await _folderAndFileService.CreateDbContextFileAsync(infrastructure, "DbContext");

                    // Repository (si se selecciona)
                    if (request.Options.UseRepository)
                    {
                        await _folderAndFileService.CreateFolderAsync(infrastructure, "Repository");
                    }

                    await Task.Yield();

                    // ------------------------------ TEST ------------------------------
                    
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (ProjectExists(solution, "Tests"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Tests' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 90,
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

                    await Task.Yield();

                    // ------------------------------ AGREGAR REFERENCIAS ---------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 95,
                        Message = "Agregando referencias entre proyectos..."
                    });

                    await _projectTemplateService.AddProjectReferenceAsync(apiProject, application);
                    await _projectTemplateService.AddProjectReferenceAsync(apiProject, infrastructure);
                    await _projectTemplateService.AddProjectReferenceAsync(infrastructure, application);
                    await _projectTemplateService.AddProjectReferenceAsync(infrastructure, domain);
                    await _projectTemplateService.AddProjectReferenceAsync(application, domain);

                    await Task.Yield();

                    // ------------------------------ AGREGAR PAQUETES NUGET ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 100,
                        Message = "Instalando Paquetes NuGet ..."
                    });

                    // Paquetes NuGet para la BD
                    if (request.Options.UserSqlServer)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(apiProject, "Microsoft.EntityFrameworkCore.SqlServer");
                    }

                    if (request.Options.UserPostgreSQL)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(apiProject, "Npgsql.EntityFrameworkCore.PostgreSQL");
                    }

                    if (request.Options.UserMongoDB)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(apiProject, "MongoDB.EntityFrameworkCore");
                    }

                    // Paquetes NuGet (EntityFrameworkCore)
                    await _projectTemplateService.AddNuGetPackageAsync(infrastructure, "Microsoft.EntityFrameworkCore");
                    await _projectTemplateService.AddNuGetPackageAsync(infrastructure, "Microsoft.EntityFrameworkCore.Relational");
                    await _projectTemplateService.AddNuGetPackageAsync(application, "Microsoft.EntityFrameworkCore");
                    await _projectTemplateService.AddNuGetPackageAsync(application, "Microsoft.EntityFrameworkCore.Relational");

                    // Paquete NuGet (AutoMapper)
                    await _projectTemplateService.AddNuGetPackageAsync(application, "AutoMapper");
                    await _projectTemplateService.AddNuGetPackageAsync(application, "AutoMapper.Extensions.Microsoft.DependencyInjection");

                    await Task.Yield();

                    // Resultado exitoso
                    return new OperationResultDto
                    {
                        Success = true,
                        Message = "Arquitectura Hexagonal creada correctamente."
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

        // Método específico para arquitectura MVC
        public async Task<OperationResultDto> CreateMvcArchitectureAsync (DTE2 dte, ArchitectureRequestDto request, IProgress<ProgressReportDto> progress)
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

                    var solution = (Solution2)dte.Solution; // Obtiene la solución actual

                    if (solution == null)
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "No hay una solución abierta."
                        };
                    }

                    if (ProjectExists(solution, request.ProjectName))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = $"El proyecto '{request.ProjectName}' ya existe en la solución."
                        };

                    }

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 25,
                        Message = "Creando proyecto MVC..."
                    });

                    // Crea el proyecto Web MVC principal
                    var mvcProject = await _projectTemplateService.CreateMvcProjectAndAddToSolutionAsync (dte, request.ProjectName);
                    var modelsFolder = mvcProject.ProjectItems.Item("Models");

                    // Creo las carpetas principales
                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 40,
                        Message = "Creando Carpetas ViewModels, DTOs, Entities..."
                    });

                    await _folderAndFileService.CreateSubFolderAsync(modelsFolder, "ViewModels");
                    await _folderAndFileService.CreateSubFolderAsync(modelsFolder, "Entities");
                    var dtos = await _folderAndFileService.CreateSubFolderAsync(modelsFolder, "DTOs");
                    await _folderAndFileService.CreateSubFolderAsync(dtos, "Requests");
                    await _folderAndFileService.CreateSubFolderAsync(dtos, "Responses");

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 50,
                        Message = "Creando Carpetas Services..."
                    });

                    var services = await _folderAndFileService.CreateFolderAsync(mvcProject, "Services");
                    await _folderAndFileService.CreateSubFolderAsync(services, "IService");
                    await _folderAndFileService.CreateSubFolderAsync(services, "Service");

                    // CQRS
                    if (request.Options.UseCQRS)
                    {
                        var commands = await _folderAndFileService.CreateSubFolderAsync(services, "Commands");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Create");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Update");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Delete");

                        var queries = await _folderAndFileService.CreateSubFolderAsync(services, "Queries");
                        await _folderAndFileService.CreateSubFolderAsync(queries, "Get");
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 60,
                        Message = "Creando Carpetas Data y Repository..."
                    });

                    var data = await _folderAndFileService.CreateFolderAsync(mvcProject, "Data");
                    await _folderAndFileService.CreateDbContextInFolderAsync(data, "DbContext",mvcProject);

                    // Repository
                    if (request.Options.UseRepository)
                    {
                        await _folderAndFileService.CreateSubFolderAsync(data, "IRepository");
                        await _folderAndFileService.CreateSubFolderAsync(data, "Repository");
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 70,
                        Message = "Creando Carpetas Helpers y Archivo AutoMapper..."
                    });

                    var helpers = await _folderAndFileService.CreateFolderAsync(mvcProject, "Helpers");

                    // AutoMapper
                    if (request.Options.UseAutoMapper)
                    {
                        await _folderAndFileService.CreateAutoMapperFileInFolderAsync(services, "AutoMapperProfiles", mvcProject);
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 80,
                        Message = "Creando Carpetas ViewComponents..."
                    });

                    await _folderAndFileService.CreateFolderAsync(mvcProject, "ViewComponents");

                    await Task.Yield();

                    // ------------------------------ TEST ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (ProjectExists(solution, "Tests"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Tests' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 90,
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

                    await Task.Yield();

                    // ------------------------------ AGREGAR PAQUETES NUGET ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 100,
                        Message = "Instalando Paquetes NuGet ..."
                    });

                    // Paquetes NuGet para la BD
                    if (request.Options.UserSqlServer)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "Microsoft.EntityFrameworkCore.SqlServer");
                    }

                    if (request.Options.UserPostgreSQL)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "Npgsql.EntityFrameworkCore.PostgreSQL");
                    }

                    if (request.Options.UserMongoDB)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "MongoDB.EntityFrameworkCore");
                    }

                    // Paquetes NuGet (EntityFrameworkCore)
                    await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "Microsoft.EntityFrameworkCore");
                    await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "Microsoft.EntityFrameworkCore.Relational");

                    // Paquete NuGet (AutoMapper)
                    await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "AutoMapper");
                    await _projectTemplateService.AddNuGetPackageAsync(mvcProject, "AutoMapper.Extensions.Microsoft.DependencyInjection");

                    await Task.Yield();

                    // Resultado exitoso
                    return new OperationResultDto
                    {
                        Success = true,
                        Message = "Arquitectura MVC creada correctamente."
                    };
                }
                catch (Exception ex)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Error al crear la arquitectura MCV.",
                        Exception = ex
                    };
                }
            });
        }

        // Método específico para arquitectura Clean
        public async Task<OperationResultDto> CreateCleanArchitectureAsync(DTE2 dte, ArchitectureRequestDto request, IProgress<ProgressReportDto> progress)
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

                    var solution = (Solution2)dte.Solution; // Obtiene la solución actual

                    if (solution == null)
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "No hay una solución abierta."
                        };
                    }

                    if (ProjectExists(solution, request.ProjectName))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = $"El proyecto '{request.ProjectName}' ya existe en la solución."
                        };

                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 25,
                        Message = "Creando proyecto Clear Architecture..."
                    });

                    // Crea el proyecto Web API principal
                    var apiProject = await _projectTemplateService.CreateWebApiProjectAndAddToSolutionAsync(dte, request.ProjectName);
                    await _folderAndFileService.CreateFolderAsync(apiProject, "Controller");

                    await Task.Yield();

                    // ------------------------------ DOMAIN ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (ProjectExists(solution, "Domain"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Domain' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 45,
                        Message = "Creando proyecto Domain..."
                    });

                    var domain = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Domain");
                    await _folderAndFileService.CreateFolderAsync(domain, "Entities");

                    await Task.Yield();
                    // ------------------------------ APPLICATION ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (ProjectExists(solution, "Application"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Application' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 65,
                        Message = "Creando proyecto Application..."
                    });

                    var application = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Application");
                    var dtosFolder = await _folderAndFileService.CreateFolderAsync(application, "DTOs");
                    await _folderAndFileService.CreateSubFolderAsync(dtosFolder, "Responses");
                    await _folderAndFileService.CreateSubFolderAsync(dtosFolder, "Requests");
                    await _folderAndFileService.CreateFolderAsync(application, "IServices");
                    await _folderAndFileService.CreateFolderAsync(application, "IRepository");
                    await _folderAndFileService.CreateAutoMapperFileAsync(application, "AutoMapperProfiles");

                    // CQRS
                    if (request.Options.UseCQRS)
                    {
                        var commands = await _folderAndFileService.CreateFolderAsync(application, "Commands");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Create");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Update");
                        await _folderAndFileService.CreateSubFolderAsync(commands, "Delete");

                        var queries = await _folderAndFileService.CreateFolderAsync(application, "Queries");
                        await _folderAndFileService.CreateSubFolderAsync(queries, "Get");
                    }

                    await Task.Yield();

                    // ------------------------------ INFRASTRUCTURE ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (ProjectExists(solution, "Infrastructure"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Infrastructure' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 85,
                        Message = "Creando proyecto Infrastructure..."
                    });

                    var infrastructure = await _projectTemplateService.CreateClassLibraryProjectAndAddToSolutionAsync(dte, "Infrastructure");
                    await _folderAndFileService.CreateDbContextFileAsync(infrastructure, "DbContext");
                    await _folderAndFileService.CreateFolderAsync(infrastructure, "Services");

                    // Repository (si se selecciona)
                    if (request.Options.UseRepository)
                    {
                        await _folderAndFileService.CreateFolderAsync(infrastructure, "Repository");
                    }

                    await Task.Yield();

                    // ------------------------------ TEST ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (ProjectExists(solution, "Tests"))
                    {
                        return new OperationResultDto
                        {
                            Success = false,
                            Message = "El proyecto 'Tests' ya existe en la solución."
                        };
                    }

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 90,
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

                    await Task.Yield();

                    // ------------------------------ AGREGAR REFERENCIAS ---------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 95,
                        Message = "Agregando referencias entre proyectos..."
                    });

                    await _projectTemplateService.AddProjectReferenceAsync(apiProject, application);
                    await _projectTemplateService.AddProjectReferenceAsync(apiProject, infrastructure);
                    await _projectTemplateService.AddProjectReferenceAsync(infrastructure, application);
                    await _projectTemplateService.AddProjectReferenceAsync(infrastructure, domain);
                    await _projectTemplateService.AddProjectReferenceAsync(application, domain);

                    await Task.Yield();

                    // ------------------------------ AGREGAR PAQUETES NUGET ------------------------------

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    progress?.Report(new ProgressReportDto
                    {
                        Percentage = 100,
                        Message = "Instalando Paquetes NuGet ..."
                    });

                    // Paquetes NuGet para la BD
                    if (request.Options.UserSqlServer)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(apiProject, "Microsoft.EntityFrameworkCore.SqlServer");
                    }

                    if (request.Options.UserPostgreSQL)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(apiProject, "Npgsql.EntityFrameworkCore.PostgreSQL");
                    }

                    if (request.Options.UserMongoDB)
                    {
                        await _projectTemplateService.AddNuGetPackageAsync(apiProject, "MongoDB.EntityFrameworkCore");
                    }

                    // Paquetes NuGet (EntityFrameworkCore)
                    await _projectTemplateService.AddNuGetPackageAsync(infrastructure, "Microsoft.EntityFrameworkCore");
                    await _projectTemplateService.AddNuGetPackageAsync(infrastructure, "Microsoft.EntityFrameworkCore.Relational");
                    await _projectTemplateService.AddNuGetPackageAsync(application, "Microsoft.EntityFrameworkCore");
                    await _projectTemplateService.AddNuGetPackageAsync(application, "Microsoft.EntityFrameworkCore.Relational");

                    // Paquete NuGet (AutoMapper)
                    await _projectTemplateService.AddNuGetPackageAsync(application, "AutoMapper");
                    await _projectTemplateService.AddNuGetPackageAsync(application, "AutoMapper.Extensions.Microsoft.DependencyInjection");

                    await Task.Yield();

                    // Resultado exitoso
                    return new OperationResultDto
                    {
                        Success = true,
                        Message = "Arquitectura Clean creada correctamente."
                    };
                }
                catch (Exception ex)
                {
                    return new OperationResultDto
                    {
                        Success = false,
                        Message = "Error al crear la Arquitectura Clean.",
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
