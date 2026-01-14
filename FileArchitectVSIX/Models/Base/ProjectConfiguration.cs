using FileArchitectLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Base
{
    public class ProjectConfiguration
    {
        public string ProjectName { get; set; } = string.Empty;
        public string SolutionName { get; set; } = string.Empty;
        public string RootPath { get; set; } = string.Empty;
        public string Language { get; set; } = "C#";
        public string Framework { get; set; } = "NET8";
        public ArchitectureType Architecture { get; set; }
        public List<PatternType> Patterns { get; set; } = new();
        //public List<ProjectTemplate> Templates { get; set; } = new();
        public Dictionary<string, string> CustomSettings { get; set; } = new();
        public bool InitializeGit { get; set; } = true;
        public bool CreateSolutionFile { get; set; } = true;
        public bool UseDocker { get; set; } = false;
    }
}
