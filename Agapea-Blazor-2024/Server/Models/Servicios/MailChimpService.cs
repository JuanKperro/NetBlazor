using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AgapeaBlazor2024.Server.Models.Servicios
{
    public class MailChimpService
    {
        public string Key { get; set; }

        MailChimpOptions chimpOptions { get; set; }

        HttpClient _httpclient { get; set; }

        MailChimpHttpClient mailChimpHttp { get; set; }
        public MailChimpService(string key)
        {
            this.Key = key;
            chimpOptions = new MailChimpOptions();
            chimpOptions.ApiKey = this.Key;
            this._httpclient = new HttpClient();
            mailChimpHttp = new MailChimpHttpClient(_httpclient, chimpOptions, "https://mandrillapp.com/api/1.0/");
        }

        public async Task EnviarEmail(string emailcliente, string subject2, string cuerpoMensaje)
        {
            //faltaria agregar un dominio DNS en la cuenta para poder que funcione... 
            HttpResponseMessage resp = await mailChimpHttp.PostAsJsonAsync<Object>("messages/send",
                new
                {
                    key = this.Key,
                    message = new { html = emailcliente, subject = subject2, from_email = "juan.palacio3@educa.madrid.org" },
                    to = new[] {
                     new { email = emailcliente }
                    }

                });
            string respString = await resp.Content.ReadAsStringAsync();
            Console.WriteLine(respString);
            //            JsonNode jsonNode = resp.Content.ReadFromJsonAsync<J() ;

        }

    }
}

