using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

public record Contact(int ContactId, string Name, string Address, string City);

public class ContactService
{
    private readonly List<Contact> _contacts = new List<Contact>
        {
            new Contact(1, "Filip W", "Bahnhofstrasse 1", "Zurich"),
            new Contact(2, "Josh Donaldson", "1 Blue Jays Way", "Toronto"),
            new Contact(3, "Aaron Sanchez", "1 Blue Jays Way", "Toronto"),
            new Contact(4, "Jose Bautista", "1 Blue Jays Way", "Toronto"),
            new Contact(5, "Edwin Encarnacion", "1 Blue Jays Way", "Toronto")
        };

    public Task<IEnumerable<Contact>> GetAll() => Task.FromResult(_contacts.AsEnumerable());

    public Task<Contact> Get(int id) => Task.FromResult(_contacts.FirstOrDefault(x => x.ContactId == id));

    public Task<int> Add(Contact contact)
    {
        var newId = (_contacts.LastOrDefault()?.ContactId ?? 0) + 1;
        _contacts.Add(new Contact(newId, contact.Name, contact.Address, contact.City));
        return Task.FromResult(newId);
    }

    public async Task Delete(int id)
    {
        var contact = await Get(id);
        if (contact == null)
        {
            throw new InvalidOperationException(string.Format("Contact with id '{0}' does not exists", id));
        }

        _contacts.Remove(contact);
    }
}