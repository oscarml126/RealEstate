using MongoDB.Driver;
using RealEstate.Application.Abstractions;
using RealEstate.Infrastructure.Configuration;
using RealEstate.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Swagger + Controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// CORS para desarrollo
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
});

// Mongo settings
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cfg = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoSettings>>().Value;
    var url = new MongoUrlBuilder(cfg.ConnectionString)
    {
        MinConnectionPoolSize = cfg.MinPoolSize,
        MaxConnectionPoolSize = cfg.MaxPoolSize
    }.ToMongoUrl();

    var settings = MongoClientSettings.FromUrl(url);
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    settings.SocketTimeout = TimeSpan.FromMilliseconds(cfg.SocketTimeoutMs);
    settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(cfg.ServerSelectionTimeoutMs);
    settings.RetryWrites = true;

    return new MongoClient(settings);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var cfg = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(cfg.DatabaseName);
});

builder.Services.AddSingleton<IPropertyRepository, PropertyRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

app.Run();
