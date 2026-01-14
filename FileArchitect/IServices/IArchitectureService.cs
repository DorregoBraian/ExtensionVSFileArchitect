using EnvDTE80;
using FileArchitectLibrary.Dtos;

namespace FileArchitectLibrary.IServices
{
    public interface IArchitectureService
    {
        public OperationResultDto CreateHexagonalArchitecture(DTE2 dte, ArchitectureRequestDto request);

    }
}
