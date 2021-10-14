using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.GenerateUrlPut.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GenerateUrlPut.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerateUrlUploadController : ControllerBase
    {
        private readonly IGenerateUrlAWSs3Service _IGenerateUrlAWSs3Service;

        public GenerateUrlUploadController(IGenerateUrlAWSs3Service generateUrlAWSs3Service)
        => _IGenerateUrlAWSs3Service = generateUrlAWSs3Service;

        [HttpGet]
        [Route("v1/GetNewUrlForUpload/{namefile}")]
        public IActionResult GetNewUrlForUpload(string namefile)
        {
            return Ok();
        }

    }
}
