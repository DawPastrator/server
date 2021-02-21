using DawPastrator.Server.Models;
using DawPastrator.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DawPastrator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataStorageController : ControllerBase
    {
        private IDataStorageService dataStorageService;

        public DataStorageController(IDataStorageService dataStorageService)
        {
            this.dataStorageService = dataStorageService;
        }

        [HttpGet]
        [Authorize]
        public async Task<DataStorageModel> GetData()
        {
            string userId = HttpContext.User.Identity?.Name ?? "";

            return await dataStorageService.Get(userId);
        }

        [HttpPost]
        [Authorize]
        public async Task Store([FromBody] DataStorageModel model)
        {
            string userId = HttpContext.User.Identity?.Name ?? "";

            await dataStorageService.Store(userId, model);
        }
    }
}
