using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Domain.Common;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.IServices
{
    public interface IDeveloperService
    {
        Task<ValueResult<PagesResult<DeveloperViewModel>>> GetAllDevelopers(int pageIndex, int pageSize);
        Task<ValueResult<DeveloperViewViewModel>> GetById(Guid id);
        Task<Result> CreateDeveloper(CreateDeveloperRequest request, CancellationToken cancellationToken);
        Task<Result> UpdateDeveloper(UpdateDeveloperRequest request, CancellationToken cancellationToken);
        Task<Result> DeleteDeveloper(Guid developerId, CancellationToken cancellationToken);
    }
}
