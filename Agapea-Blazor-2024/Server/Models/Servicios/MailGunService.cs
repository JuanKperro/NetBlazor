using Azure;
using MailChimp.Net.Core;
using MailChimp.Net.Models;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace AgapeaBlazor2024.Server.Models.Servicios
{
    public class MailGunService
    {
        private string key { get; set; }

        public MailGunService(string key)
        {
            this.key = key;
        }

        public async Task SendComplexMessage(string emailcliente, string subject, string cuerpoMensaje)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.mailgun.net/v3");

            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;

            client.DefaultRequestHeaders.Add("api", key);
            HttpAuthorization httpAuthorization = new HttpAuthorization("api", "ss");


            //request.AddParameter("domain", "sandbox30f3a72e5f7540ba8cfcff63c94308ec.mailgun.org", ParameterType.UrlSegment);
            //request.Resource = "{domain}/messages";
            request.Properties.Add("from", "Excited User sandbox30f3a72e5f7540ba8cfcff63c94308ec.mailgun.org");
            request.Properties.Add("to", emailcliente);
            request.Properties.Add("subject", subject);
            request.Properties.Add("html",
                                  cuerpoMensaje);
            HttpResponseMessage resp = await client.SendAsync(request);
            string repString = await resp.Content.ReadAsStringAsync();
            Console.Out.WriteLine(repString);

        }
    }
}
