using EnvDTE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileArchitectVSIX.IServices
{
    public interface IFolderAndFileService
    {
        Task<ProjectItem> CreateFolderAsync (Project project, string folderName);
        Task<ProjectItem> CreateSubFolderAsync (ProjectItem parentFolder, string folderName);
        Task CreateAutoMapperFileAsync (Project project, string fileName);
        Task CreateAutoMapperFileInFolderAsync(ProjectItem parentFolder, string contextName, Project project);
        Task CreateDbContextFileAsync (Project project, string fileName);
        Task CreateDbContextInFolderAsync(ProjectItem parentFolder, string contextName, Project project);
    }
}
