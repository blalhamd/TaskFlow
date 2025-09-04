using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Core.Models.ViewModels.V1;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.IServices
{
    public interface IDeveloperService
    {
        Task<PagesResult<DeveloperViewModel>> GetAllDevelopers(int pageIndex, int pageSize);
        Task<DeveloperViewViewModel> GetById(Guid id);
        Task<bool> CreateDeveloper(CreateDeveloperRequest request, CancellationToken cancellationToken);
        Task<bool> UpdateDeveloper(UpdateDeveloperRequest request, CancellationToken cancellationToken);
        Task<bool> DeleteDeveloper(Guid developerId, CancellationToken cancellationToken);
    }
}
