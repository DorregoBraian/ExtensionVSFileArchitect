using EnvDTE;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.IServices
{
    public interface IFolderAndFileService
    {
        public ProjectItem CreateFolder(Project project, string folderName);
        public ProjectItem CreateSubFolder(ProjectItem parentFolder, string folderName);
        public void CreateAutoMapperFile(Project project, string fileName);
        public void CreateDbContextFile(Project project, string fileName);
    }
}
