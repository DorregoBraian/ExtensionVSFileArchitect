using FileArchitectLibrary.Dtos.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Dtos
{
    public class ProjectSpecDto
    {
        // Nombre corto del proyecto, por ejemplo "Domain", "Application", "Infrastructure", "Api"
        public string ProjectSuffix { get; set; } = string.Empty;

        // Tipo de proyecto a crear (ClassLibrary, WebApi, MVC, etc.) - lo puedes mapear a string o enum
        public ArchitectureType TemplateId { get; set; }

        // Carpetas iniciales que querés crear dentro del proyecto
        public List<string> Folders { get; set; } = new List<string>();
    }
}
