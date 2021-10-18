using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Model.GenerateUrl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GenerateUrlPut.Interfaces
{
    public interface IGenerateUrlAWSs3Service
    {
        public string GenerateUrl(string namefile, VerbSelecionado verboSelecionado);

        public string ExecuteprocessUpload(IFormFileCollection files, string url);
    }
}
