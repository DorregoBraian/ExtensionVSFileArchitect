using EnvDTE;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectVSIX.IServices
{
    public interface IFolderAndFileService
    {
        ProjectItem CreateFolder(Project project, string folderName);
        ProjectItem CreateSubFolder(ProjectItem parentFolder, string folderName);
        void CreateAutoMapperFile(Project project, string fileName);
        void CreateDbContextFile(Project project, string fileName);
    }
}
