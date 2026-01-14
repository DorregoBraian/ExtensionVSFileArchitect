using FileArchitectLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.UI
{
    public class UserInput
    {
        public string ProjectName { get; set; } = string.Empty;
        public string SolutionName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public ArchitectureType SelectedArchitecture { get; set; }
        public List<PatternType> SelectedPatterns { get; set; } = new();
        public ProjectType ProjectType { get; set; }
        public string FrameworkVersion { get; set; } = "NET8";

        // Opciones avanzadas
        public bool UseDocker { get; set; }
        public bool UseGit { get; set; } = true;
        public bool CreateSolution { get; set; } = true;
        public bool IncludeTests { get; set; } = true;
        public bool IncludeLogging { get; set; } = true;
        public bool IncludeSwagger { get; set; } = true;

        // Customizaciones
        public Dictionary<string, string> CustomFolderNames { get; set; } = new();
        public List<string> AdditionalLayers { get; set; } = new();

        // Validación
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ProjectName) &&
                   !string.IsNullOrWhiteSpace(Location);
        }
    }
}
