using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using HospitalPortalWasm;
using HospitalPortalWasm.Services;




var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


// Persistencia en navegador + servicio de dominio
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IHospitalService, HospitalService>();


builder.Services.AddBlazoredLocalStorage();

// Servicios de dominio (uno central para simplificar el prototipo)
builder.Services.AddScoped<IHospitalService, HospitalService>();

await builder.Build().RunAsync();
