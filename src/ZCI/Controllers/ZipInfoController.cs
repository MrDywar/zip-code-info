using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZCI.Common.DTO;
using ZCI.Domain;

namespace ZCI.Controllers
{
    [ApiController]
    public class ZipInfoController : ControllerBase
    {
        private readonly IZipInfoService _zipInfoService;

        public ZipInfoController(IZipInfoService zipInfoService)
        {
            _zipInfoService = zipInfoService;
        }

        [Route("api/zipinfo"), HttpGet]
        public async Task<ZipCodeInfoDTO> Get(string zipCode)
        {
            return await _zipInfoService.GetZipCodeInfoAsync(zipCode);
        }
    }
}