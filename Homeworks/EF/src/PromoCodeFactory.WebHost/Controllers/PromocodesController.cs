using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController
        : ControllerBase
    {
        private readonly IRepository<PromoCode> _promocodeRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<Preference> _preferenseRepository;
        private readonly IMapper _mapper;

        public PromocodesController(IRepository<PromoCode> promocodeRepository,ICustomerRepository customerRepository, IRepository<Preference> preferenseRepository,IMapper mapper)
        {
            _promocodeRepository = promocodeRepository;
            _customerRepository = customerRepository;
            _preferenseRepository = preferenseRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить все промокоды 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// GetPromocodesAsync - здесь даты передаются строками, чтобы не было проблем с часовыми поясами ??
        /// Возвращаем все промокоды. Предполагаем, что имелась ввиду конвертация дат BeginDate и EndDate в строку.
        /// </remarks>
        /// 

        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            //TODO: Получить все промокоды 
            var promocodes = await _promocodeRepository.GetAllAsync();
            if (promocodes == null)
            {
                return NotFound("Список промокодов пуст");
            }
            var promocodesListAll = _mapper.Map<List<PromoCodeShortResponse>>(promocodes);
            return Ok(promocodesListAll);
        }

        /// <summary>
        /// Создать промокод и выдать его клиентам с указанным предпочтением
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Метод GivePromocodesToCustomersWithPreferenceAsync должен сохранять новый промокод в базе данных и находить клиентов с переданным предпочтением и добавлять им данный промокод.
        /// </remarks>
        /// 
        [HttpPost]
        public async  Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            //TODO: Создать промокод и выдать его клиентам с указанным предпочтением
            var promocodes = _promocodeRepository.GetAllAsync().Result;
            if (promocodes.Any(x => x.Code == request.PromoCode))
                return BadRequest($"Промокод '{request.PromoCode}' уже существует!");

            var customers = await _customerRepository.GetAllWithDetailsAsync();
            if (customers == null)
                return NotFound("Список клиентов пуст");

            var preferenses = await _preferenseRepository.GetAllAsync();
            var preference = preferenses.FirstOrDefault(x => x.Name == request.Preference);
            if (preference == null)
            {
                return NotFound($"Предпочтение '{request.Preference}' не найдено");
            }

            var customersWithPreference = customers
                .Where(c => c.CustomerPreferences?
                    .Any(cp => cp.Preference?.Name == request.Preference) ?? false)
                .ToList();
            if (customersWithPreference.Count == 0)
                return NotFound("Клиенты с указанным предпочтением отсутствуют");
            // Создаем промокод
            var promoCode = _mapper.Map<PromoCode>(request, opts =>
            {
                opts.Items["Preference"] = preference; 
                opts.Items["PreferenceId"] = preference.Id;
            });

            await _promocodeRepository.CreateAsync(promoCode);

            foreach (var customer in customersWithPreference)
            {
                customer.Promocodes ??= new List<PromoCode>();
                customer.Promocodes.Add(promoCode);
          //      await _customerRepository.UpdateAsync(customer);
            }
            await _customerRepository.SaveChangesAsync();
            await _promocodeRepository.SaveChangesAsync();

            return Ok("Промокод выдан клиентам с указанным предпочтением");
        }
    }
}