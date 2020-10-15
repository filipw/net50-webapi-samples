using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// with endpoint routing and JSON endpoints
// see https://github.com/dotnet/aspnetcore/pull/23496/ for more
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
    });
}).Build().Run();