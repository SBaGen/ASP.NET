using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeesController(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Получить данные всех сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            var employeesModelList = employees.Select(x =>
                new EmployeeShortResponse()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                }).ToList();

            return employeesModelList;
        }

        /// <summary>
        /// Получить данные сотрудника по Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound();

            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return employeeModel;
        }
        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>

        [HttpPost]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployeeAsync([FromBody] Employee employee)//Model Binding
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee),"Не заданы данные нового сотрудника");
            }
            var newEmployee = await _employeeRepository.CreateAsync(employee);

            return CreatedAtAction(nameof(CreateEmployeeAsync), newEmployee);
        }
        /// <summary>
        /// Обновление данных сотрудника 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] Employee employee) //Model Binding
        {
            if (employee == null || employee.Id != id)
            {
                return BadRequest("Не заданы данные сотрудника для обновления"); // Ошибка! HTTP-ответ с кодом состояния Bad Request (400) 
            }

            try
            {
                await _employeeRepository.UpdateAsync(employee);
                return NoContent(); //Успех! HTTP-ответ с кодом состояния No Content (204).
            }
            catch (InvalidOperationException exception)
            {
                return NotFound(exception.Message); // Ошибка! HTTP-ответ с кодом состояния Not Found (404) 
            }
        }
        /// <summary>
        /// Удаление сотрудника 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmployeeAsync(Guid id) {
            try
            { 
                await _employeeRepository.DeleteAsync(id);
                return NoContent();//Успех! HTTP-ответ с кодом состояния No Content (204).
            }
            catch (InvalidOperationException exception)
            {
                return NotFound(exception.Message); // Ошибка! HTTP-ответ с кодом состояния Not Found (404) 
            }
        }


    }
}