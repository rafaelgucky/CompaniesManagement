using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using API.Repositories.Interfaces;
using API.Repositories;
using API.DTOs;
using AutoMapper;
using API.Pagination;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CompaniesController(IUnitOfWork unitOfWork, ILogger<CompaniesController> logger, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("Pagination")]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetAsync([FromQuery] CompanyParameters parameters)
        {
            var companies = await _unitOfWork.CompanyRepository.PaginationGetAsync(parameters);
            var metadata = new
            {
                companies.PageNumber,
                companies.PageSize,
                companies.Count,
                companies.PageCount,
                companies.HasNextPage,
                companies.HasPreviousPage,
            };
            Response.Headers.Append("X-Companies", JsonConvert.SerializeObject(metadata));
            return _mapper.Map<IEnumerable<CompanyDTO>>(companies).ToList();
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetAsync()
        {
            IEnumerable<Company>? companies = await _unitOfWork.CompanyRepository.GetAsync();
            if (companies == null) return Ok();
            IEnumerable<CompanyDTO> companiesDTO = _mapper.Map<IEnumerable<CompanyDTO>>(companies);
            return companiesDTO.ToList();
        }

        [HttpGet("{id:int:min(1)}", Name = "Companies")]
        public async Task<ActionResult<CompanyDTO>> GetAsync(int id)
        {
            Company? company = await _unitOfWork.CompanyRepository.GetAsync(company => company.Id == id);
            if(company == null) return NotFound();
            return _mapper.Map<CompanyDTO>(company);
        }
        /*
        [HttpGet("{id:int:min(1)}/employees")]
        public ActionResult<CompanyDTO> GetEmployees(int id)
        {
            Company? company = _unitOfWork.CompanyRepository.GetEmployees(id);
            if (company is null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return company;

        }
        [HttpGet("{id:int:min(1)}/products")]
        public ActionResult<CompanyDTO> GetProducts(int id)
        {
            Company? company = _unitOfWork.CompanyRepository.GetProducts(id);
            if (company is null) return NotFound();
            return company;
        }
        */
        [HttpPost]
        public async Task<ActionResult> CreateAsync(CompanyDTO company)
        {
            _unitOfWork.CompanyRepository.Create(_mapper.Map<Company>(company));
            await _unitOfWork.CommitAsync();
            return new CreatedAtRouteResult("Companies", new { Id = company.Id }, company);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CompanyDTO>> UpdateAsync(int id, CompanyDTO company)
        {
            if (id != company.Id)
            {
                return BadRequest();
            }
            _unitOfWork.CompanyRepository.Update(_mapper.Map<Company>(company));
            await _unitOfWork.CommitAsync();
            return Ok(company);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<CompanyDTO>> DeleteAsync(int id)
        {
            //Company? company = await _companiesServices.Delete(id);
            Company? company = _unitOfWork.CompanyRepository.Delete(id);
            await _unitOfWork.CommitAsync();
            return Ok(_mapper.Map<CompanyDTO>(company));
        }
    }
}
