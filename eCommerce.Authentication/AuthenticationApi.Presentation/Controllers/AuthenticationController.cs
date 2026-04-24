using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
   public class AuthenticationController(IUser userInterface) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(AppUserDTO appUserDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await userInterface.Register(appUserDTO);
            return result.Flag ? Ok(result) : BadRequest(result);
        }
        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userInterface.Login(loginDTO);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetUserDTO>> GetUser(int id)
        {
            if (id <= 0) return BadRequest("Invalid user Id");

            var user = await userInterface.GetUser(id);

            // Check if user is null first!
            if (user == null) return NotFound("User not found");

            return user.id > 0 ? Ok(user) : NotFound();
        }
    }
}
