using AutoMapper;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System;

namespace PromoCodeFactory.WebHost.Mapping
{
    public class PromocodeProfiler:Profile
    {
        public PromocodeProfiler()
        {
            CreateMap<GivePromoCodeRequest, PromoCode>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.PromoCode))
                .ForMember(dest => dest.ServiceInfo, opt => opt.MapFrom(src => src.ServiceInfo))
                .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.PartnerName))
                .ForMember(dest => dest.BeginDate, opt => opt.MapFrom(_ => DateTime.Now))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(_ => DateTime.Now.AddDays(30)))
                .ForMember(dest => dest.Preference, opt => opt.MapFrom((src, dest, _, context) =>
                    context.Items["Preference"] as Preference))
                .ForMember(dest => dest.PreferenceId, opt => opt.MapFrom((src, dest, _, context) =>
                    (Guid)context.Items["PreferenceId"]))
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.PartnerManager, opt => opt.Ignore())
                .ForMember(dest => dest.PartnerManagerId, opt => opt.Ignore());



            CreateMap<PromoCode, PromoCodeShortResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.ServiceInfo, opt => opt.MapFrom(src => src.ServiceInfo))
                .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.PartnerName))
                .ForMember(dest => dest.BeginDate, opt => opt.MapFrom(src => src.BeginDate.ToString("dd.MM.yyyy")))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString("dd.MM.yyyy")));

        }
    }
    
}
