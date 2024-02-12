using AgapeaBlazor2024.Client;
using AgapeaBlazor2024.Client.Models.Services;
using AgapeaBlazor2024.Client.Models.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("AgapeaBlazor2024.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AgapeaBlazor2024.ServerAPI"));

//definicion de inyeccion de servicios propios.....
builder.Services.AddScoped<IRestService, MiRestService>();
builder.Services.AddScoped<IStorageService, SubjectStorageService>();

await builder.Build().RunAsync();
