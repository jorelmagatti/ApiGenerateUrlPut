using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Model.GenerateUrl;
using Services.GenerateUrlPut.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace API.GenerateUrlPut.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerateUrlUploadController : ControllerBase
    {
        #region Private Parameters
        private readonly IGenerateUrlAWSs3Service _IGenerateUrlAWSs3Service;
        private readonly ILogger _logger;
        #endregion

        #region Constructor
        public GenerateUrlUploadController(IGenerateUrlAWSs3Service generateUrlAWSs3Service, ILogger<GenerateUrlUploadController> logger)
        {
            _IGenerateUrlAWSs3Service = generateUrlAWSs3Service;
            _logger = logger;
        }
        #endregion

        #region Actions 
        [HttpGet]
        [Route("v1/GetNewUrlForUpload/{namefile}")]
        public IActionResult GetNewUrlForUpload(string namefile, string verb)
        {
            try
            {
                if (!string.IsNullOrEmpty(namefile) && !string.IsNullOrEmpty(verb))
                {
                    if (validaExtensaoArquivo(namefile))
                    {
                        return ExecutaGenerateUrl(namefile, verb);
                    }
                    else
                    {
                        _logger.LogWarning("parametros de entrada invalidos, nome do arquvo sem extenção");
                        return BadRequest("parametros de entrada invalidos, nome do arquvo sem extenção");
                    }
                }
                else
                {
                    _logger.LogWarning("parametros de entrada invalidos, sem valores para processo");
                    return BadRequest("parametros de entrada invalidos, sem valores para processo");
                }

                #region Internal Methods
                IActionResult ExecutaGenerateUrl(string namefile, string verbo)
                {
                    string retornoUrl = string.Empty;
                    if (verbo.Trim().ToLower().Contains("get"))
                    {
                        retornoUrl = _IGenerateUrlAWSs3Service.GenerateUrl(namefile, VerbSelecionado.Get);
                        if (!string.IsNullOrEmpty(retornoUrl))
                            return Ok(retornoUrl);
                        else
                            return NotFound();
                    }
                    else if (verbo.Trim().ToLower().Contains("put"))
                    {
                        retornoUrl = _IGenerateUrlAWSs3Service.GenerateUrl(namefile, VerbSelecionado.Put);
                        if (!string.IsNullOrEmpty(retornoUrl))
                            return Ok(retornoUrl);
                        else
                            return NotFound();
                    }
                    else
                        return NotFound();
                }

                bool validaExtensaoArquivo(string filename)
                {
                    bool validacao = string.IsNullOrEmpty(Path.GetExtension(filename));
                    if (validacao)
                        return false;
                    else
                        return true;
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("v1/PutFileForUploadURL")]
        public IActionResult PutFileForUploadURL()
        {
            try
            {
                _logger.LogInformation("Iniciando processo de captura dos parametros referente processo upload");
                IFormFileCollection files = Request.Form.Files;
                string url = GetFormPostString("url");

                if (files != null && !string.IsNullOrEmpty(url))
                {
                    if (files.Count() > 0)
                    {
                        var retorno = _IGenerateUrlAWSs3Service.ExecuteprocessUpload(files, url);
                        if (retorno.Trim().ToLower().Contains("ok"))
                            return Ok(retorno);
                        else
                            return NotFound(retorno);
                    }
                    else
                    {
                        _logger.LogWarning($"Falha no recebimento dos parametros necessários");
                        return BadRequest("Falha no recebimento dos parametros necessários");
                    }
                }
                else
                {
                    _logger.LogWarning($"Falha no recebimento dos parametros necessários");
                    return BadRequest("Falha no recebimento dos parametros necessários");
                }

                #region Internal Methods
                string GetFormPostString(string formControlName)
                {
                    string response = string.Empty;
                    StringValues formValues;
                    if (Request.Form.TryGetValue(formControlName, out formValues))
                        response = formValues.First().ToString();
                    return response;
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        #endregion

    }
}
