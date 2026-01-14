using FileArchitectLibrary.Models.Base;
using FileArchitectLibrary.Models.Enums;
using FileArchitectLibrary.Models.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Models.Builders
{
    public class ProjectBuilderConfig
    {
        public UserInput UserInput { get; set; } = new();
        public ArchitectureDefinition Architecture { get; set; } = new();
        public List<PatternDefinition> Patterns { get; set; } = new();
        public List<TemplateFile> Templates { get; set; } = new();
        public List<FileToGenerate> FilesToGenerate { get; set; } = new();

        public static ProjectBuilderConfig CreateDefault()
        {
            return new ProjectBuilderConfig
            {
                Architecture = new ArchitectureDefinition
                {
                    Type = ArchitectureType.Layered,
                    Name = "Default Layered Architecture"
                }
            };
        }
    }
}
