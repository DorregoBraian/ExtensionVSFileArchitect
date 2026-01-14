using FileArchitectLibrary.Dtos.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Dtos
{
    public class ArchitectureRequestDto
    {
        // Tipo de arquitectura seleccionada (Hexagonal, Clean, MVC, etc.)
        public ArchitectureType Architecture { get; set; }

        // Ej: "MyCompany.MyApp"
        public string BaseNamespace { get; set; } = string.Empty;

        // Opciones desde la UI (checkboxes)
        public ArchitectureOptionsDto Options { get; set; } = new ArchitectureOptionsDto();

        // Nombre base para los proyectos (prefijo para Project names)
        // Ej: si BaseProjectName = "MyApp", resultará en "MyApp.Domain", "MyApp.Application", ...
        //public string BaseProjectName { get; set; } = "MyApp";

        // Especificaciones opcionales por proyecto (puedes dejar vacío y el service usa defaults)
        //public List<ProjectSpecDto> ProjectSpecs { get; set; } = new List<ProjectSpecDto>();
    }
}
