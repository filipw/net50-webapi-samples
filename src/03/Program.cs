using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// with endpoint routing and JSON endpoints
// see https://github.com/dotnet/aspnetcore/pull/23496/ for more on the JSON helpers
WebHost.CreateDefaultBuilder().
ConfigureServices(s => s.AddSingleton<ContactService>()).
Configure(app => 
{
    app.UseRouting();
    app.UseEndpoints(e => 
    {
        var contactService = e.ServiceProvider.GetRequiredService<ContactService>();
        e.MapGet("/contacts", async c => 
            await c.Response.WriteAsJsonAsync(await contactService.GetAll()));
        e.MapGet("/contacts/{id:int}", async c => 
            await c.Response.WriteAsJsonAsync(await contactService.Get(int.Parse((string)c.Request.RouteValues["id"]))));
        e.MapPost("/contacts", async c => 
        {
            await contactService.Add(await c.Request.ReadFromJsonAsync<Contact>());
            c.Response.StatusCode = 201;
        });
        e.MapDelete("/contacts/{id:int}", async c => 
        {
            await contactService.Delete(int.Parse((string)c.Request.RouteValues["id"]));
            c.Response.StatusCode = 204;
        });
    });
}).Build().Run();