using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

// simplest
WebHost.CreateDefaultBuilder().Configure(app => app.Run(c => c.Response.WriteAsync("Hello world!"))).Build().Run();