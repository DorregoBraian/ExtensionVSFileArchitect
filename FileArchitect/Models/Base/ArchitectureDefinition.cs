using FileArchitectLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Base
{
    public class ArchitectureDefinition
    {
        public ArchitectureType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProjectLayer> Layers { get; set; } = new();
        public List<PatternType> RequiredPatterns { get; set; } = new();
        public List<PatternType> OptionalPatterns { get; set; } = new();
        public Dictionary<string, string> DefaultFiles { get; set; } = new();
        public Dictionary<string, List<string>> FolderStructure { get; set; } = new();
    }

    public class ProjectLayer
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<LayerFolder> Folders { get; set; } = new();
        public List<string> DefaultFiles { get; set; } = new();
        public bool IsOptional { get; set; } = false;
    }

    public class LayerFolder
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> FileTemplates { get; set; } = new();
        public List<LayerFolder> SubFolders { get; set; } = new();
        public bool IsOptional { get; set; } = false;
    }

}
