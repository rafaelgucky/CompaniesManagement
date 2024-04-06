using API.Data;
using API.DTOs;
using API.Models;
using API.Pagination;
using API.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("Pagination")]
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
            return _mapper.Map<IEnumerable<EmployeeDTO>>(employees).ToList();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> GetAsync()
        {
            var employees = await _unitOfWork.EmployeeRepository.GetAsync();
            return _mapper.Map<IEnumerable<EmployeeDTO>>(employees).ToList();
        }

        [HttpGet("{id:int:min(1)}", Name = "GetById")]
        public async Task<ActionResult<EmployeeDTO>> GetAsync(int id)
        {
            Employee? employee = await _unitOfWork.EmployeeRepository.GetAsync(employee => employee.Id == id);
            if(employee == null)
            {
                return NotFound();
            }
            return _mapper.Map<EmployeeDTO>(employee);
        }

        [HttpGet("{name:alpha}")]
        public ActionResult<IEnumerable<EmployeeDTO>> GetByName(string name)
        {
            IEnumerable<Employee>? employees = _unitOfWork.EmployeeRepository.GetByName(name);
            if (employees == null)
            {
                return NotFound();
            }
            return _mapper.Map<IEnumerable<EmployeeDTO>>(employees).ToList();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(EmployeeDTO employee)
        {
            _unitOfWork.EmployeeRepository.Create(_mapper.Map<Employee>(employee));
            await _unitOfWork.CommitAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult<EmployeeDTO>> Update(int id, EmployeeDTO employee)
        {
            if (id != employee.Id) return NotFound();
            _unitOfWork.EmployeeRepository.Update(_mapper.Map<Employee>(employee));
            await _unitOfWork.CommitAsync();
            return new CreatedAtRouteResult("GetById", new { Id = employee.Id }, employee);
        }

        [HttpDelete]
        public async Task<ActionResult<EmployeeDTO>> DeleteAsync(int id)
        {
            Employee employee = _unitOfWork.EmployeeRepository.Delete(id);
            await _unitOfWork.CommitAsync();
            return Ok(_mapper.Map<EmployeeDTO>(employee));
        }
    }
}
