using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Rhetos;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
//builder.Host.ConfigureLogging(cfg =>
//{
//    cfg.AddConsole();
//    cfg.AddDebug();
//});
builder.Host.UseNLog();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.ToString());
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TestApp", Version = "v1" });
    c.SwaggerDoc("rhetos", new OpenApiInfo { Title = "Rhetos REST API", Version = "v1" });
});

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
       // options.JsonSerializerOptions.MaxDepth = 2;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        })
    ;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => o.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    });

builder.Services
    .AddRhetosHost((serviceProvider, rhetosHostBuilder) =>
    {
        rhetosHostBuilder
            .ConfigureRhetosAppDefaults()
            .UseBuilderLogProviderFromHost(serviceProvider)            
            .ConfigureConfiguration(cfg =>
            {                
                cfg.MapNetCoreConfiguration(builder.Configuration);
            });
    })
    .AddAspNetCoreIdentityUser()
    .AddHostLogging()
    .AddDashboard()
    .AddRestApi(o =>
    {
        o.BaseRoute = "rest";        
        o.GroupNameMapper = (conceptInfo, controller, oldName) => "rhetos";
    })
    ;

var app = builder.Build();
app.AddNLogWeb();

app.UseRhetosRestApi();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/rhetos/swagger.json", "Rhetos REST API");
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestApp v1");
    });
    app.MapRhetosDashboard("dash");
}
app.UseRouting();


//app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    if (app.Environment.IsDevelopment())
    {
        endpoints.MapRhetosDashboard();
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.Run();


