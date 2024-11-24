using Autofac;
using Hub.Infrastructure.Database;
using Hub.Infrastructure.Tasks;
using Hub.Infrastructure;
using System.Reflection;
using Hub.Infrastructure.Autofac.Dependency;
using Hub.API;
using Autofac.Extensions.DependencyInjection;

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

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Inicializa o Engine com a configuração do HubProvider e outros parâmetros
    Engine.Initialize(
        executingAssembly: Assembly.GetExecutingAssembly(),
        nameProvider: new HubProvider(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>()), 
        tasks: new List<IStartupTask>()
        {
            new StartupTask(),
        },
        dependencyRegistrars: new List<IDependencySetup>()
        {
            // Adicione os registradores de dependência aqui
        },
        containerBuilder: containerBuilder,
        csb: new ConnectionStringBaseVM()
        {
            ConnectionStringBaseSchema = "sch",
            ConnectionStringNhAssembly = "Hub.Core.dll"
        });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
