using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileArchitectVSIX.IServices
{
    public interface IProjectTemplateService
    {
        Task<Project> CreateClassLibraryProjectAndAddToSolutionAsync (DTE2 dte, string projectName);
        Task<Project> CreateWebApiProjectAndAddToSolutionAsync (DTE2 dte, string projectName);
        Task<Project> CreateTestProjectAndToSolutionAsync (DTE2 dte, string projectName);
        Task CreateMvcProjectAndAddToSolutionAsync (DTE2 dte, string projectName);
        Task AddProjectReferenceAsync (Project from, Project to);



    }
}
