using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.GenerateUrl;
using Services.GenerateUrlPut.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.GenerateUrlPut.Service
{
    public class GenerateUrlAWSs3Service : IGenerateUrlAWSs3Service
    {
        #region Private Parameter
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly IAmazonS3 client;
        private readonly IConfiguration _Configuration;
        private readonly string usuarioSecreto = string.Empty;
        private readonly string chaveSecreta = string.Empty;
        private readonly string bucket = string.Empty;
        private readonly int tempoExpiracaoUrl = 0;
        #endregion

        #region Constructor
        public GenerateUrlAWSs3Service(IConfiguration configuration, ILogger<GenerateUrlAWSs3Service> logger)
        {
            _Configuration = configuration;
            _logger = logger;
            usuarioSecreto = _Configuration["AWS:S3Acess:KeyId"];
            chaveSecreta = _Configuration["AWS:S3Acess:SecretKey"];
            bucket = _Configuration["AWS:S3Acess:bucket"];
            tempoExpiracaoUrl = int.Parse(_Configuration["AWS:S3Acess:expireminutes"]);
            client = new AmazonS3Client(usuarioSecreto, chaveSecreta, Amazon.RegionEndpoint.USEast1);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Método para gerar URL pre assinada de acordo com parametros recebidos
        /// </summary>
        /// <param name="filename">string para receber o nomedo arquivo para gerar Url para o mesmo</param>
        /// <param name="verboSelecionado">enum com parametro de tipo de verbo (Put, get) para gerar a url</param>
        /// <returns>retorno em string a url para efetuar requizição de acordo com verbo selecionado</returns>
        public string GenerateUrl(string filename, VerbSelecionado verboSelecionado)
        {
            try
            {
                switch (verboSelecionado)
                {
                    case VerbSelecionado.Put:
                        return GenerateUrlForVerb(HttpVerb.PUT, bucket, filename);
                    case VerbSelecionado.Get:
                        return GenerateUrlForVerb(HttpVerb.GET, bucket, filename);
                    default:
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ex.Message;
            }
        }

        public string ExecuteprocessUpload(IFormFileCollection files, string url)
        {
            try
            {
                var file = files.First();
                string content = file.ContentType;
                Stream fileStream = file.OpenReadStream();

                _logger.LogInformation("Finalizado processo de captura dos parametros referente processo upload");
                _logger.LogInformation("Iniciando processo de request do upload para URL assinada");
                var retorno = UploadArquivoViaUrlpreAssinadaS3(url, fileStream);
                if (retorno.Trim().ToLower().Contains("ok"))
                {
                    _logger.LogInformation($"Upload Efetuado com sucesso, resposta do processo: {retorno}");
                    return retorno;
                }
                else
                {
                    _logger.LogWarning($"Falha ao tentar efetuar upload do arquivo, respostas : {retorno}");
                    return retorno;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ex.Message;
            }
        }
        #endregion

        #region Private Methods
        private string GenerateUrlForVerb(HttpVerb verb, string bucket, string FileNameKey)
        => client.GetPreSignedURL(new GetPreSignedUrlRequest()
        {
            BucketName = bucket
            ,Key = FileNameKey
            ,Verb = verb
            ,Expires = DateTime.Now.AddMinutes(tempoExpiracaoUrl)
        });

        /// <summary>
        /// Métod para efetuar Upload de umarquivo via web requizição Put
        /// </summary>
        /// <param name="url">string para receber a url pre assinada para requizição de upload</param>
        /// <param name="file">objeto do Tipo Stream contendo binário do arquivo selecionado para upload</param>
        /// <returns>retorna uma string contendo o status response da web requizição enviada</returns>
        private string UploadArquivoViaUrlpreAssinadaS3(string url, Stream file)
        {
            try
            {
                byte[] data = ConverteStreamToByteArray(file);

                HttpWebRequest httpRequest = WebRequest.Create(url) as HttpWebRequest;
                httpRequest.Method = "PUT";
                httpRequest.ContentLength = data.Length;

                Stream requestStream = httpRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                HttpWebResponse response2 = httpRequest.GetResponse() as HttpWebResponse;
                return response2.StatusCode.ToString();

                #region Internal Methods
                byte[] ConverteStreamToByteArray(Stream stream)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ex.Message;
            }
        }
        #endregion
    }
}
