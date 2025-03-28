using AutoMapper;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Mapping
{
    public class EmployeeProfile:Profile
    {
        public EmployeeProfile()
        {
            // Маппинг для роли
            CreateMap<Role, RoleItemResponse>();

            // Маппинг сотрудника
            CreateMap<Employee, EmployeeResponse>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));
        }
    }
}
