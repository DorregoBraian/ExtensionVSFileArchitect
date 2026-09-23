using FileArchitectVSIX.Dtos.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectVSIX.Dtos
{
    public class ArchitectureRequestDto
    {
        // Tipo de arquitectura seleccionada (Hexagonal, Clean, MVC, etc.)
        public ArchitectureType Architecture { get; set; }

        // Ej: "MyCompany.MyApp"
        public string NameSpace { get; set; } = string.Empty;

        // Ej: "MyApp"
        public string ProjectName { get; set; } = "MyApp";

        // Opciones desde la UI (checkboxes)
        public ArchitectureOptionsDto Options { get; set; } = new ArchitectureOptionsDto();


    }
}
