using UserAccessManager.Services.Apicurio;
using UserAccessManager.Services.HttpClients;
using UserAccessManager.Services.Kafka;
using Serilog;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient(typeof(ApiCurioRegistryClient).Name, httpClient =>
{
    httpClient.BaseAddress = new Uri("http://dems-apicurioregistry-kafkasql.5b7aa5-test.router-default.apps.silver.devops.gov.bc.ca");
    

});
var config = InitializeConfiguration(builder.Services);

AccessRequestConfiguration InitializeConfiguration(IServiceCollection services)
{
    var config = new AccessRequestConfiguration();
    builder.Configuration.Bind(config);
    services.AddSingleton(config);

    Log.Logger.Information("### App Version:{0} ###", Assembly.GetExecutingAssembly().GetName().Version);
    Log.Logger.Information("### Access Request Service Configuration:{0} ###", JsonSerializer.Serialize(config));

    return config;
}

builder.Services.AddKafkaConsumer(config)
    .AddHttpClients(config)
    .AddSingleton(new RetryPolicy())
    .AddSingleton<Microsoft.Extensions.Logging.ILogger>(svc => svc.GetRequiredService<ILogger<UserProvisioningHandler>>());

builder.Services.AddOptions();

builder.Services.Add(new(typeof(ApiCurioRegistryClient), typeof(ApiCurioRegistryClient), ServiceLifetime.Scoped));

builder.Services.Add(new(typeof(IApiCurioRegistryClient), provider => provider.GetRequiredService<ApiCurioRegistryClient>(), ServiceLifetime.Scoped));


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
