using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Application.DTOs.Conversions;
using ProductApi.Application.Interface;

namespace ProductApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProduct productInterface) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            //get all product from repo
            var products = await productInterface.GetAllAsync();
            if (!products.Any())
            
                return NotFound("No product detected in the database");

            //convert data from entity to dto and return
            var (_, list) = ProductConversion.FromEntity(null!, products);
            return list !.Any() ? Ok(list) : NotFound("No product detected in the database");
            
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            //get single product from repo
            var product = await productInterface.FindByIdAsync(id);
            if (product is null)
                return NotFound("No product detected in the database");

            //convert from entity to dto and return
            var (_product, _) = ProductConversion.FromEntity(product, null!);
            return _product is not null ? Ok(_product) : NotFound("Product not Found");
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateProduct(ProductDTO product)
        {
            //check model state if all data annotations are passed
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            //convert to entity
            var getEntity  = ProductConversion.ToEntity(product);
            var response = await productInterface.CreateAsync(getEntity);
            return response.Flag is true ? Ok(response) : BadRequest(response);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateProduct(ProductDTO product)
        {
            //check model state if all data annotations are passed
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            //convert to entity
            var getEntity = ProductConversion.ToEntity(product);
            var response = await productInterface.UpdateAsync(getEntity);
            return response.Flag is true ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<Response>> DeleteProduct(ProductDTO product)
        {
            //Convert to entity
            var getEntity = ProductConversion.ToEntity(product);
            var response = await productInterface.DeleteAsync(getEntity);
            return response.Flag is true ? Ok(response) : BadRequest(response);
        }
    }
}
