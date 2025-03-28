using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Abstractions.Repositories;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<PromoCode> _promocodeRepository;
        private readonly IRepository<Preference> _preferenceRepository;
        private readonly IRepository<CustomerPreference> _customerPreferenceRepository;


        public CustomersController(ICustomerRepository customerRepository,
            IRepository<PromoCode> promocodeRepository, IRepository<Preference> preferenceRepository, IRepository<CustomerPreference> customerPreferenceRepository, IMapper mapper)
        {
            _customerRepository = customerRepository;
            _promocodeRepository = promocodeRepository;
            _preferenceRepository = preferenceRepository;
            _customerPreferenceRepository = customerPreferenceRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить список всех клиентов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CustomerShortResponse>>> GetCustomersAsync()
        {
            //TODO: Добавить получение списка клиентов
            var customers = await _customerRepository.GetAllAsync();
            if (customers == null || !customers.Any())
                return NotFound();

            return Ok(_mapper.Map<List<CustomerShortResponse>>(customers));

//            return Ok(customerShortResponse);
        }
        /// <summary>
        /// Получить клиента по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetWithDetailsByIdAsync(id);
            if (customer == null)
                return NotFound("Клиент с таким идентификатором не найден");

            var response = _mapper.Map<CustomerResponse>(customer);
            return Ok(response);
        }

        /// <summary>
        /// Создание нового клиента вместе с его предпочтениями
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            //TODO: Добавить создание нового клиента вместе с его предпочтениями
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var customer = _mapper.Map<Customer>(request, opts =>
            {
                opts.Items["PreferenceRepository"] = _preferenceRepository;
            });
            try
            {
                await _customerRepository.CreateAsync(customer);
                await _customerRepository.SaveChangesAsync();
                return Ok(_mapper.Map<CustomerResponse>(customer));
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Произошла ошибка при сохранении данных: {ex.Message}");
            }
        }
        /// <summary>
        /// Редактирование данных клиента вместе с его предпочтениями
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            //TODO: Обновить данные клиента вместе с его предпочтениями
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            // Удаляем старые предпочтения
            var cp = await _customerPreferenceRepository.GetAllAsync();
            if (cp != null || !cp.Any())
            {
                var oldsPref = cp.Where(x => x.CustomerId == customer.Id).ToList();
                if (oldsPref != null && oldsPref.Any())
                {
                    oldsPref.ToList().ForEach(x => _customerPreferenceRepository.DeleteAsync(x).Wait());
                }
            }
            // Обновляем данные клиента вместе с его предпочтениями
            _mapper.Map(request,customer, opts =>{opts.Items["PreferenceRepository"] = _preferenceRepository;});
            try
            {
                // Сохраняем клиента в базу данных
                await _customerRepository.UpdateAsync(customer);
                await _customerRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Обработка ошибок базы данных
                return BadRequest("Произошла ошибка при сохранении данных:" + ex.Message);
            }
            return Ok();
        }
        /// <summary>
        /// Удаление клиента вместе с выданными ему промокодами и предпочтениями
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return NotFound();
            try
            {
                //TODO: Удаление клиента вместе с выданными ему промокодами
                await _customerRepository.DeleteAsync(customer);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Произошла ошибка при сохранении данных:" + ex.Message);
            }
            return Ok();
        }

    }
}