using FileArchitectLibrary.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Base
{
    public class TemplateFile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public TemplateType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public string FileExtension { get; set; } = ".cs";
        public Dictionary<string, TemplateParameter> Parameters { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string TargetArchitecture { get; set; } = "Any";
    }

    public class TemplateParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public ParameterType Type { get; set; }
    }
}
