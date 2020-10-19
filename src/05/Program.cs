using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

WebHost.CreateDefaultBuilder().
ConfigureServices(s =>
{
    s.AddIdentityServer().AddTestConfig();
    s.AddSingleton<ContactService>();
    s.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().
            AddAuthenticationSchemes("Bearer").
            RequireAuthenticatedUser().
            RequireClaim("scope", "read").
            Build();
    })
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = "http://localhost:5000/openid";
        o.Audience = "embedded";
        o.RequireHttpsMetadata = false;
    });
}).
Configure(app =>
{
    app.UseRouting();
    app.Map("/openid", id =>
    {
        // use embedded identity server to issue tokens
        id.UseIdentityServer();
    });
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(e =>
    {
        var contactService = e.ServiceProvider.GetRequiredService<ContactService>();

        e.MapGet("/contacts",
            async c => await c.Response.WriteAsJsonAsync(await contactService.GetAll()));
        e.MapGet("/contacts/{id:int}",
            async c => await c.Response.WriteAsJsonAsync(await contactService.Get(int.Parse((string)c.Request.RouteValues["id"]))));
        e.MapPost("/contacts",
            async c =>
            {
                await contactService.Add(await c.Request.ReadFromJsonAsync<Contact>());
                c.Response.StatusCode = 201;
            });
        e.MapDelete("/contacts/{id:int}",
            async c =>
            {
                await contactService.Delete(int.Parse((string)c.Request.RouteValues["id"]));
                c.Response.StatusCode = 204;
            });
    });
}).Build().Run();