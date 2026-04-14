using Contatos.Api.Extensions;
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

app.UseSwagger(); //Swagger
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
