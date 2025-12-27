using AutoMapper;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOs;
using KS_Sweets.Application.Contracts.DTOs.CategoryDTOS;
using KS_Sweets.Domain.Entities;

namespace KS_Sweets.Application.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CategoryCreateDto, Category>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src =>
                    src.Name.ToLower().Trim().Replace(" ", "-")));
            CreateMap<CategoryEditDto, Category>()
                 .ForMember(d => d.ImageUrl, opt => opt.Ignore())
                 .ForMember(d => d.CreatedAt, opt => opt.Ignore()).ReverseMap();
        }
    }
}