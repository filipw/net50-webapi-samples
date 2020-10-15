using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// with endpoint routing and JSON endpoints
// see https://github.com/dotnet/aspnetcore/pull/23496/ for more on the JSON helpers
WebHost.CreateDefaultBuilder().
ConfigureServices(s => s.AddSingleton<InMemoryContactRepository>()).
Configure(app => 
{
    app.UseRouting();
    app.UseEndpoints(e => 
    {
        e.MapGet("/contacts", async c => 
            await c.Response.WriteAsJsonAsync(await c.RequestServices.GetRequiredService<InMemoryContactRepository>().GetAll()));
        e.MapGet("/contacts/{id:int}", async c => 
            await c.Response.WriteAsJsonAsync(await c.RequestServices.GetRequiredService<InMemoryContactRepository>().Get(int.Parse((string)c.Request.RouteValues["id"]))));
        e.MapPost("/contacts", async c => 
        {
            var repo = c.RequestServices.GetRequiredService<InMemoryContactRepository>();
            await repo.Add(await c.Request.ReadFromJsonAsync<Contact>());
            c.Response.StatusCode = 201;
        });
        e.MapDelete("/contacts/{id:int}", async c => 
        {
            var repo = c.RequestServices.GetRequiredService<InMemoryContactRepository>();
            await repo.Delete(int.Parse((string)c.Request.RouteValues["id"]));
            c.Response.StatusCode = 204;
        });
    });
}).Build().Run();