using Microsoft.OpenApi.Models;
using Rhetos;

var builder = WebApplication.CreateBuilder(args);

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
        options.JsonSerializerOptions.MaxDepth = 2;
        })
    ;

builder.Services
    .AddRhetosHost((serviceProvider, rhetosHostBuilder) =>
    {
        rhetosHostBuilder
            .ConfigureRhetosAppDefaults()
            .UseBuilderLogProviderFromHost(serviceProvider)            
            .ConfigureConfiguration(cfg => cfg.MapNetCoreConfiguration(builder.Configuration));
    })
    .AddAspNetCoreIdentityUser()
    .AddHostLogging()
    //.AddDashboard()
    .AddRestApi(o =>
    {
        o.BaseRoute = "rest";
        o.GroupNameMapper = (conceptInfo, controller, oldName) => "rhetos";
    })
    ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/rhetos/swagger.json", "Rhetos REST API");
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestApp v1");
    });
   // app.MapRhetosDashboard("dash");
}
app.MapControllers();
app.UseRhetosRestApi();
//app.UseAuthorization();


app.Run();


