using Hub.Infrastructure.Database;
using Hub.Infrastructure.Tasks;
using Hub.Infrastructure;
using System.Reflection;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.Infrastructure.Localization;
using Hub.Infrastructure.Resources;
using Hub.Application.Resource;
using Hub.API.Configuration;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Hub.API.Configuration.Context;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

if (builder.Environment.IsDevelopment())
{
    foreach (var item in builder.Configuration.AsEnumerable().Where(c => c.Key.StartsWith("Settings:")))
    {
        Environment.SetEnvironmentVariable(item.Key.Replace("Settings:", ""), item.Value);
    }
}

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ITenantContext, TenantContext>();

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Inicializa o Engine com a configuração do HubProvider e outros parâmetros
    Engine.Initialize(
        executingAssembly: Assembly.GetExecutingAssembly(),
        tasks: new List<IStartupTask>()
        {
            new StartupTask(),
        },
        dependencyRegistrars: new List<IDependencyConfiguration>()
        {
            new Hub.Application.Configuration.DependencyConfiguration(),
        },
        containerBuilder: containerBuilder,
        csb: new ConnectionStringBaseVM()
        {
            ConnectionStringBaseSchema = "sch",
            ConnectionStringNhAssembly = "Hub.Domain.dll"
        });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var app = builder.Build();

Engine.SetContainer((IContainer)app.Services.GetAutofacRoot());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var startupTasks = new List<Task>();

startupTasks.Add(Task.Run(() =>
{
    Engine.Resolve<DefaultLocalizationProvider>().RegisterWrapper(new ResourceWrapper(typeof(TextResource).Assembly, "Resource.TextResource"));
}));

app.Run();
