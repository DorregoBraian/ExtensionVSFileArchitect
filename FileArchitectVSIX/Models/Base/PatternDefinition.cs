using FileArchitectLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Base
{
    public class PatternDefinition
    {
        public PatternType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PatternFile> Files { get; set; } = new();
        public List<PatternDependency> Dependencies { get; set; } = new();
        public Dictionary<string, string> ImplementationRules { get; set; } = new();
    }

    public class PatternFile
    {
        public string TemplateName { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
        public bool IsInterface { get; set; } = false;
        public string BaseClass { get; set; } = string.Empty;
    }

    public class PatternDependency
    {
        public PatternType RequiredPattern { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
