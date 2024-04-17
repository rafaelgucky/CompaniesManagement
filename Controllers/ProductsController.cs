using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Repositories.Interfaces;
using API.Repositories;
using AutoMapper;
using API.DTOs;
using API.Pagination;
using Newtonsoft.Json;
using API.Pagination.Shared;
using X.PagedList;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;


namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiversion}/[controller]")]
    [Produces("application/json")]
    //[EnableRateLimiting("fixed")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateAsync(ProductDTO product)
        {
            Product productCreated = _unitOfWork.ProductRepository.Create(_mapper.Map<Product>(product));
            if (productCreated == null) return BadRequest();
            await _unitOfWork.CommitAsync();
            return new CreatedAtRouteResult("products", new { Id = productCreated.Id }, productCreated);
        }

        [HttpGet("pagination")]
        public async Task<ActionResult<ProductDTO>> GetAsync([FromQuery] ProductsParameters parameters)
        {
            var products = await _unitOfWork.ProductRepository.PaginationGetAsync(parameters);
            var productsDTO = _mapper.Map<IEnumerable<ProductDTO>>(products);

            var metadata = new
            {
                products.PageNumber,
                products.PageSize,
                products.Count,
                products.PageCount,
                products.HasNextPage,
                products.HasPreviousPage,
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(productsDTO);
        }
        [HttpGet("filter/price")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAsync([FromQuery] ProductsPriceFilter filter)
        {
            IPagedList<Product> products = await _unitOfWork.ProductRepository.PriceFilterAsync(filter);

            var metadata = new
            {
                products.PageNumber,
                products.PageSize,
                products.Count,
                products.PageCount,
                products.HasNextPage,
                products.HasPreviousPage,
            };
            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            return Ok(_mapper.Map<IEnumerable<ProductDTO>>(products));
        }

        /// <summary>
        /// Obtém um objeto do tipo Product com base no id informado
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Um objeto de tipo Product</returns>

        [HttpGet("{id:int:min(1)}", Name = "products")]
        public async Task<ActionResult<ProductDTO>> GetByIdAsync(int id)
        {
            Product? product = await _unitOfWork.ProductRepository.GetAsync(product => product.Id == id);
            if(product is null) return NotFound();
            return Ok(_mapper.Map<ProductDTO>(product));
        }

        /// <summary>
        /// Obtém todos os objetos do tipo Product da base de dados
        /// </summary>
        /// <returns>Uma lista de objetos Product</returns>

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAsync()
        {
            IEnumerable<Product> products = await _unitOfWork.ProductRepository.GetAsync();
            return Ok(_mapper.Map<IEnumerable<ProductDTO>>(products).ToList());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductDTO>> UpdateAsync(int id, ProductDTO product)
        {
            if (id != product.Id) return BadRequest();
            var productUpdated =  _unitOfWork.ProductRepository.Update(_mapper.Map<Product>(product));
            if (productUpdated == null) return BadRequest();
            await _unitOfWork.CommitAsync();
            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProductDTO>> DeleteAsync(int id)
        {
            var product = _unitOfWork.CompanyRepository.GetAsync(product => product.Id == id);
            if (product == null) return NotFound();
            if (!_unitOfWork.ProductRepository.Delete(id)) return BadRequest();
            await _unitOfWork.CommitAsync();
            return Ok("Deleted");
        }

    }
}
