using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Dtos
{
    public class ArchitectureOptionsDto
    {
        public bool UseRepository { get; set; }
        public bool UseCQRS { get; set; }
        public bool UseUnitOfWork { get; set; }
        public bool UseAutoMapper { get; set; }
    }
}
