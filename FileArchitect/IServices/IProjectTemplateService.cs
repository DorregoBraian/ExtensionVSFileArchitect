using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.IServices
{
    public interface IProjectTemplateService
    {
        public Project CreateClassLibraryProjectAndAddToSolution(DTE2 dte, string projectName);
        public void CreateWebApiProjectAndAddToSolution(DTE2 dte, string projectName);
        public void CreateMvcProjectAndAddToSolution(DTE2 dte, string projectName);
        public void AddProjectReference(Project from, Project to);

    }
}
