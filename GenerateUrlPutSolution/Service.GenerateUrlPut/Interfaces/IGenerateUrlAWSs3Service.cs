using Amazon.S3;
using Model.GenerateUrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GenerateUrlPut.Interfaces
{
    public interface IGenerateUrlAWSs3Service
    {
        public string GenerateUrl(string namefile, VerbSelecionado verboSelecionado);
    }
}
