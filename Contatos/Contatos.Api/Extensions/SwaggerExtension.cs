using Microsoft.OpenApi;

namespace Contatos.Api.Extensions;

public static class SwaggerExtension
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CadastroDeContatos API",
                Version = "v1",
                Description = "API RESTful de cadastro de usuários com autenticação JWT."
            });

            // Define o esquema (Botão Authorize global)
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira APENAS o token JWT."
            });

            // A MÁGICA ESTÁ AQUI: repare que agora o "document" é passado
            // para dentro do OpenApiSecuritySchemeReference, resolvendo o bug do Swagger.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document),
                    []
                }
            });
        });

        return services;
    }
}