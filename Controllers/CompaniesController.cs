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
using Asp.Versioning;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")] 
    // [ApiVersion("1.0", Deprecated = true)] Difinir versão como depreciada/obsoleta
    [Route("v{version:apiversion}/[controller]")]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
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

        [HttpGet("pagination")]
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
            return Ok(_mapper.Map<IEnumerable<CompanyDTO>>(companies).ToList());
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetAsync()
        {
            IEnumerable<Company>? companies = await _unitOfWork.CompanyRepository.GetAsync();
            IEnumerable<CompanyDTO> companiesDTO = _mapper.Map<IEnumerable<CompanyDTO>>(companies);
            return Ok(companiesDTO.ToList());
        }

        [HttpGet("{id:int:min(1)}", Name = "Companies")]
        public async Task<ActionResult<CompanyDTO>> GetAsync(int id)
        {
            Company? company = await _unitOfWork.CompanyRepository.GetAsync(company => company.Id == id);
            if(company == null) return NotFound();
            return Ok(_mapper.Map<CompanyDTO>(company));
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(CompanyDTO company)
        {
            var companyCreated = _unitOfWork.CompanyRepository.Create(_mapper.Map<Company>(company));
            if (companyCreated == null) return BadRequest();
            await _unitOfWork.CommitAsync();
            return new CreatedAtRouteResult("Companies", new { Id = companyCreated.Id }, companyCreated);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CompanyDTO>> UpdateAsync(int id, CompanyDTO company)
        {
            if (id != company.Id) return BadRequest();
            var companyUpdated = _unitOfWork.CompanyRepository.Update(_mapper.Map<Company>(company));
            if (companyUpdated == null) return BadRequest();
            await _unitOfWork.CommitAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var company = _unitOfWork.CompanyRepository.GetAsync(company => company.Id == id);
            if(company == null) return NotFound();
            if(!_unitOfWork.CompanyRepository.Delete(id)) return BadRequest();
            await _unitOfWork.CommitAsync();
            return Ok("Deleted");
        }
    }
}
