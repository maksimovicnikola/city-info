using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers(options => { options.ReturnHttpNotAcceptable = true; })
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

// This part will allow us to add additional information within the Error object on a server level
//builder.Services.AddProblemDetails(options =>
//{
//    options.CustomizeProblemDetails = ctx =>
//    {
//        ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional info example");
//        ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);
//    };
//});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// This will ensure that files are downloaded with their real extension without the extension definition
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();
// builder.Services.AddTransient() - These services are created each time they are requested; Works best for leightweigh, stateless services
// builder.Services.AddScoped() -  Created once per request.
// builder.Services.AddSingleton() - Created just the first time they are requested, every other request will use the same instance.
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>();
builder.Services.AddDbContext<CityInfoContext>(dbContextOptions =>
    dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]));

// Scoped should be used for Repositories
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();
// Registering AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // Configure how to validate a token
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"] ?? string.Empty))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();