using TaskFlow.Core.IRepositories.Non_Generic;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data.context;
using TaskFlow.Infrastructure.Repositories.Generic;

namespace TaskFlow.Infrastructure.Repositories.Non_Generic
{
    public class DeveloperRepositoryAsync : GenericRepositoryAsync<Developer>, IDeveloperRepositoryAsync
    {
        public DeveloperRepositoryAsync(AppDbContext context) : base(context)
        {
        }
    }
}
