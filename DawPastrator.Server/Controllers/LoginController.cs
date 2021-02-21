using DawPastrator.Server.Models;
using DawPastrator.Server.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DawPastrator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly IDawAuthenticationService dawAuthenticationService;

        public LoginController(IDawAuthenticationService dawAuthenticationService)
        {
            this.dawAuthenticationService = dawAuthenticationService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = StringConstant.Cookies)]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync();
        }

        // POST api/<LoginController>
        [HttpPost]
        public async Task Login([FromBody] UserLoginModel model)
        {

            if (dawAuthenticationService.Verify(model, out var principal))
            {
                await HttpContext.SignInAsync(StringConstant.Cookies, principal);
            }
            
        }

        //// GET api/<LoginController>/5/6
        //[HttpGet("{id}/{id2}")]
        //public async Task<string> Get(int id, int id2)
        //{
        //    return "value" + id + id2;
        //}

        //// PUT api/<LoginController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<LoginController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
