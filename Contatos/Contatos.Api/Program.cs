using Contatos.Api.Extensions;
using Contatos.Api.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddRouting(map => map.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer(); //Swagger
builder.Services.AddSwaggerGen(); //Swagger

builder.Services.AddInfraStructure(builder.Configuration);
builder.Services.AddDomainService(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(); //Swagger

//Scalar
app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.Mars);
});

app.MapOpenApi();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
