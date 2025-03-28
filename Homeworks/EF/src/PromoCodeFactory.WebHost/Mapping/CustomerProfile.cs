using AutoMapper;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using PromoCodeFactory.Core.Abstractions.Repositories;

namespace PromoCodeFactory.WebHost.Mapping
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            // Маппинг из Customer в CustomerShortResponse
            CreateMap<Customer, CustomerShortResponse>();

            // Маппинг для CustomerPreference
            CreateMap<CustomerPreference, Preference>()
                .ForMember(dest => dest.CustomerPreferences, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PreferenceId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Preference.Name));

            // Маппинг из Customer в CustomerResponse
            CreateMap<Customer, CustomerResponse>()
                .ForMember(dest => dest.Preferences, opt => opt.MapFrom(src =>
                    src.CustomerPreferences != null
                        ? src.CustomerPreferences.Select(cp => cp.Preference)
                        : null))
                .ForMember(dest => dest.PromoCodes, opt => opt.MapFrom(src =>
                    src.Promocodes));

            CreateMap<Preference, PreferenceResponse>();
            // Маппинг для создания/редактирования
            CreateMap<CreateOrEditCustomerRequest, Customer>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom((src, dest, _) => dest.Id == Guid.Empty ? Guid.NewGuid() : dest.Id))
                .ForMember(dest => dest.Promocodes, opt => opt.MapFrom(_ => new List<PromoCode>()))
                .ForMember(dest => dest.CustomerPreferences, opt => opt.MapFrom((src, dest, member, context) =>
                {
                    if (src.PreferenceIds == null || !src.PreferenceIds.Any())
                        return null;
                    // Получаем репозиторий из контекста DI
                    var preferenceRepo = (IRepository<Preference>)context.Items["PreferenceRepository"];
                    if (preferenceRepo == null)
                        return null;
                    var preferencesAll = preferenceRepo.GetAllAsync().Result;
                    var preferences = preferencesAll
                        .Where(p => src.PreferenceIds.Contains(p.Id))
                        .ToList();
                    return preferences.Select(p => new CustomerPreference
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = dest.Id,
                        PreferenceId = p.Id
                    }).ToList();
                }));

        }
    }
}
