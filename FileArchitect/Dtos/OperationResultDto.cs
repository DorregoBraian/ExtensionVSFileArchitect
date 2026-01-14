using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectLibrary.Dtos
{
    public class OperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Exception Exception { get; set; } = null;
    }

}
