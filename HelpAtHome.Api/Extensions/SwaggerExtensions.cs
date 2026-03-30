using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace HelpAtHome.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title   = "HelpAtHome API",
                    Version = "v1",
                    Description = """
                        ## HelpAtHome REST API

                        Connects **clients**, **caregivers**, and **agencies** on the HelpAtHome platform.

                        ### Authentication
                        Most endpoints require a valid JWT Bearer token.
                        Obtain one via `POST /api/auth/login`, then click **Authorize** and paste the token.

                        ### Roles
                        | Role | Description |
                        |---|---|
                        | `Client` | End user looking for care services |
                        | `IndividualCaregiver` | Self-employed caregiver |
                        | `AgencyCaregiver` | Caregiver employed by an agency |
                        | `AgencyAdmin` | Manages an agency and its caregivers |
                        | `Admin` | Platform administrator |
                        | `SuperAdmin` | Unrestricted platform access |

                        ### Common Response Codes
                        | Code | Meaning |
                        |---|---|
                        | `200` | Success |
                        | `201` | Resource created |
                        | `400` | Validation error or business rule violation |
                        | `401` | Missing or invalid JWT token |
                        | `403` | Valid token but insufficient role/policy |
                        | `404` | Resource not found |
                        """,
                    Contact = new OpenApiContact
                    {
                        Name  = "HelpAtHome Support",
                        Email = "support@helpathome.ng"
                    },
                    License = new OpenApiLicense { Name = "Proprietary" }
                });

                // ── JWT Bearer security definition ────────────────────────────────
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name        = "Authorization",
                    Type        = SecuritySchemeType.Http,
                    Scheme      = "bearer",
                    BearerFormat = "JWT",
                    In          = ParameterLocation.Header,
                    Description = "Paste your JWT access token (without the `Bearer ` prefix — Swagger adds it automatically)."
                });

                // Add padlock icon + 401/403 to every protected operation
                options.OperationFilter<AuthorizeOperationFilter>();

                // Show enum member names (not raw integers) in schemas and examples
                options.SchemaFilter<EnumSchemaFilter>();

                // Include XML doc comments produced by the build
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                // Sort operations by tag then path for a predictable reading order
                options.OrderActionsBy(a => $"{a.GroupName}_{a.RelativePath}_{a.HttpMethod}");

                // Avoid schema ID conflicts from same-named types in different namespaces
                options.CustomSchemaIds(t => t.FullName?.Replace('+', '.'));
            });

            return services;
        }

        public static WebApplication UseSwaggerConfiguration(this WebApplication app)
        {
            app.UseSwagger(c => c.RouteTemplate = "swagger/{documentName}/swagger.json");

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "HelpAtHome API v1");
                options.RoutePrefix      = "swagger";
                options.DocumentTitle    = "HelpAtHome API";
                options.DisplayRequestDuration();        // show how long each request took
                options.EnablePersistAuthorization();    // keep the token between page refreshes
                options.DefaultModelsExpandDepth(-1);    // collapse the schema section by default
                options.DefaultModelExpandDepth(3);      // expand nested models up to 3 levels
                options.EnableDeepLinking();             // shareable links per operation
                options.EnableFilter();                  // search/filter box at the top
                options.ShowExtensions();
                options.EnableValidator();
            });

            return app;
        }
    }

    /// <summary>
    /// Adds the padlock icon and 401/403 response descriptions to every
    /// operation that requires authorization, while leaving anonymous endpoints
    /// (AllowAnonymous or no Authorize attribute) untouched.
    /// </summary>
    internal sealed class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // AllowAnonymous on the method always wins
            if (context.MethodInfo.GetCustomAttributes<AllowAnonymousAttribute>(true).Any())
                return;

            var controllerHasAuthorize = context.MethodInfo.DeclaringType!
                .GetCustomAttributes<AuthorizeAttribute>(true).Any();
            var methodHasAuthorize = context.MethodInfo
                .GetCustomAttributes<AuthorizeAttribute>(true).Any();

            if (!controllerHasAuthorize && !methodHasAuthorize)
                return;

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            if (!operation.Responses.ContainsKey("401"))
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized – JWT token is missing or invalid."
                });

            if (!operation.Responses.ContainsKey("403"))
                operation.Responses.Add("403", new OpenApiResponse
                {
                    Description = "Forbidden – authenticated but lacking the required role or policy."
                });
        }
    }

    /// <summary>
    /// Replaces raw integer enum values in Swagger schemas with their string
    /// member names, making request examples and schema descriptions readable.
    /// </summary>
    internal sealed class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum) return;

            schema.Enum.Clear();
            schema.Type   = "string";
            schema.Format = null;

            foreach (var name in Enum.GetNames(context.Type))
                schema.Enum.Add(new OpenApiString(name));
        }
    }
}
