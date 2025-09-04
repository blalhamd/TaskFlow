using AutoMapper;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Core.AutoMapper
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            //CreateMap<CreateDeveloperRequest, Developer>()
            //    .ConstructUsing(dto=> new Developer(dto.FullName,dto.Age,dto.ImagePath,dto.JobTitle,dto.YearOfExperience,dto.JobLevel,dto.UserId));
        }
    }
}
