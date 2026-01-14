using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskFlow.Core.IRepositories.Non_Generic;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data.context;
using TaskFlow.Infrastructure.Repositories.Generic;

namespace TaskFlow.Infrastructure.Repositories.Non_Generic
{
    public class TaskRepositoryAsync : GenericRepositoryAsync<TaskEntity>, ITaskRepositoryAsync
    {
        public TaskRepositoryAsync(AppDbContext context) : base(context)
        {
        }

        public async Task<TaskDto?> FirstOrDefaultAsync(Expression<Func<TaskEntity, bool>> predicate)
        {
            var task = await _repo.Where(predicate).Select(x => new TaskDto
            {
                CommentDtos = x.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    FullName = c.Developer.FullName
                }).ToList()
            })
            .FirstOrDefaultAsync();

            return task;
        }
    }
}
