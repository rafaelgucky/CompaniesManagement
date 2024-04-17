using API.Data;
using API.DTOs;
using API.Models;
using API.Pagination;
using API.Repositories.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiversion}/[controller]")]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetAsync([FromQuery] EmployeeParameters parameters)
        {
            var employees = await _unitOfWork.EmployeeRepository.PaginationGetAsync(parameters);
            var metadata = new
            {
                employees.PageNumber,
                employees.PageSize,
                employees.Count,
                employees.PageCount,
                employees.HasNextPage,
                employees.HasPreviousPage,
            };
            Response.Headers.Append("X-Companies", JsonConvert.SerializeObject(metadata));
            return Ok(_mapper.Map<IEnumerable<EmployeeDTO>>(employees).ToList());
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetAsync()
        {
            var employees = await _unitOfWork.EmployeeRepository.GetAsync();
            return Ok(_mapper.Map<IEnumerable<EmployeeDTO>>(employees).ToList());
        }

        [HttpGet("{id:int:min(1)}", Name = "employees")]
        public async Task<ActionResult<EmployeeDTO>> GetAsync(int id)
        {
            Employee? employee = await _unitOfWork.EmployeeRepository.GetAsync(employee => employee.Id == id);
            if (employee == null) return NotFound();
            return Ok(_mapper.Map<EmployeeDTO>(employee));
        }

        [HttpGet("{name:alpha}")]
        public ActionResult<IEnumerable<EmployeeDTO>> GetByName(string name)
        {
            IEnumerable<Employee>? employees = _unitOfWork.EmployeeRepository.GetByName(name);
            //if (employees == null) return NotFound();
            return Ok(_mapper.Map<IEnumerable<EmployeeDTO>>(employees).ToList());
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(EmployeeDTO employee)
        {
            var employeeCreated = _unitOfWork.EmployeeRepository.Create(_mapper.Map<Employee>(employee));
            if (employee == null) return BadRequest();
            await _unitOfWork.CommitAsync();
            return new CreatedAtRouteResult("employees", new { Id = employeeCreated.Id }, employeeCreated);
        }

        [HttpPut("{id:min(1)}")]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmployeeDTO>> Update(int id, EmployeeDTO employee)
        {
            if (id != employee.Id) return NotFound();
            var employeeUpdated = _unitOfWork.EmployeeRepository.Update(_mapper.Map<Employee>(employee));
            if (employeeUpdated == null) return BadRequest();
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        [HttpDelete("{id:min(1)}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmployeeDTO>> DeleteAsync(int id)
        {
            var employee = _unitOfWork.CompanyRepository.GetAsync(employee => employee.Id == id);
            if (employee == null) return NotFound();
            if (!_unitOfWork.EmployeeRepository.Delete(id)) return BadRequest();
            await _unitOfWork.CommitAsync();
            return Ok();
        }
    }
}
