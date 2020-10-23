using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

// with endpoint routing
WebHost.CreateDefaultBuilder().Configure(app => 
{
    app.UseRouting();
    app.UseEndpoints(e => 
    {
        e.MapGet("/", c => c.Response.WriteAsync("Hello world!"));
        e.MapGet("hello/{name}", c => c.Response.WriteAsync($"Hello, {c.Request.RouteValues["name"]}"));
    });
}).Build().Run();