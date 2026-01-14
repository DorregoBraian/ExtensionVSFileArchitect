using FileArchitectLibrary.Models.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Templates
{
    public class RepositoryTemplate : TemplateFile
    {
        public string EntityName { get; set; } = string.Empty;
        public bool IncludeInterface { get; set; } = true;
        public bool IncludeGeneric { get; set; } = true;
        public List<string> Methods { get; set; } = new()
        {
            "GetById",
            "GetAll",
            "Add",
            "Update",
            "Delete",
            "Exists"
        };
    }
}
