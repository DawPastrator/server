using DawPastrator.Server.Models;
using DawPastrator.Server.Services;
using DawPastrator.Server.Utils;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            if (HttpContext.User.TryGetUserId(out var userId))
            {
                return await dataStorageService.GetAsync(userId);
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        [Authorize]
        public async Task Store([FromBody] DataStorageModel model)
        {
            if (HttpContext.User.TryGetUserId(out var userId))
            {
                await dataStorageService.StoreAsync(userId, model);
            }
        }
    }
}
