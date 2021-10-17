using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.GenerateUrl;
using Services.GenerateUrlPut.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
        #endregion
    }
}
