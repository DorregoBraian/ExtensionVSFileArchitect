using FileArchitectLibrary.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Templates
{
    public class ServiceTemplate : TemplateFile
    {
        public string ServiceName { get; set; } = string.Empty;
        public bool IncludeInterface { get; set; } = true;
        public List<ServiceMethod> Methods { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
    }

    public class ServiceMethod
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "void";
        public List<MethodParameter> Parameters { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class MethodParameter
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
