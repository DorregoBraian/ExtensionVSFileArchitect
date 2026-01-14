using EnvDTE80;
using FileArchitectVSIX.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileArchitectVSIX.IServices
{
    public interface IArchitectureService
    {
        OperationResultDto CreateHexagonalArchitecture(DTE2 dte, ArchitectureRequestDto request);

    }
}
