using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vertikal.Core.Interfaces
{
    public interface IApiClient
    {
        void SetBearerToken(string token);
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
