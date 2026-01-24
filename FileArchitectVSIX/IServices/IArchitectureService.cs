using EnvDTE80;
using FileArchitectVSIX.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileArchitectVSIX.IServices
{
    public interface IArchitectureService
    {
        Task<OperationResultDto> CreateHexagonalArchitectureAsync (DTE2 dte, ArchitectureRequestDto request, IProgress<ProgressReportDto> progress);
        //Task<OperationResultDto> CreateClearArchitectureAsync (DTE2 dte, ArchitectureRequestDto request, IProgress<ProgressReportDto> progress);
        //Task<OperationResultDto> CreateMvcArchitectureAsync (DTE2 dte, ArchitectureRequestDto request, IProgress<ProgressReportDto> progress);

    }
}
