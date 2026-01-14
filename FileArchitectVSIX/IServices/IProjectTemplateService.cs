using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectVSIX.IServices
{
    public interface IProjectTemplateService
    {
        Project CreateClassLibraryProjectAndAddToSolution(DTE2 dte, string projectName);
        void CreateWebApiProjectAndAddToSolution(DTE2 dte, string projectName);
        void CreateMvcProjectAndAddToSolution(DTE2 dte, string projectName);
        void AddProjectReference(Project from, Project to);

    }
}
