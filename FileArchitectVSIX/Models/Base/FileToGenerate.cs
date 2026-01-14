using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Base
{
    public class FileToGenerate
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
        public bool OverwriteIfExists { get; set; } = false;
        //public FileType FileType { get; set; }
    }
}
