using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Fileharbor.Common;
using Fileharbor.Common.Configuration;
using Fileharbor.Common.Database;
using Fileharbor.Common.Utilities;
using Fileharbor.Services;
using Fileharbor.Services.Contracts;
using Fileharbor.Services.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Swashbuckle.AspNetCore.Swagger;

namespace Fileharbor
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly IOptions<AuthenticationConfiguration> _authenticationConfiguration;
        private readonly SigningCredentials _signingCredentials;

        public Startup(IConfiguration configuration,
            IHostingEnvironment environment,
            IOptions<AuthenticationConfiguration> authenticationConfiguration)
        {
            _authenticationConfiguration = authenticationConfiguration;
            _configuration = configuration;
            _environment = environment;

            _signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authenticationConfiguration.Value.SigningCredentials)),
                SecurityAlgorithms.HmacSha256);
            
            // dapper needs to be configured to use a custom mapper for all columns
            Dapper.SqlMapper.SetTypeMap(typeof(UserEntity), new ColumnNameAttributeTypeMapper<UserEntity>());
        }
        
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            // allow lazy resolution of dependencies
            services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));

            // register the database context
            services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(_configuration["Database:ConnectionString"]));

            // this is the default principal populated by the CurrentPrincipalMiddleware
            services.AddScoped<CurrentPrincipal>();

            // register all the services
            services.AddScoped<IUserService, UserService>()
                .AddScoped<ICollectionService, CollectionService>()
                .AddScoped<ICollectionTemplateService, CollectionTemplateService>()
                .AddScoped<IPermissionService, PermissionService>()
                .AddScoped<IColumnService, ColumnService>()
                .AddScoped<IContentTypeService, ContentTypeService>();

            // add mvc
            services.AddMvc();

            // configure swagger
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                
                // TODO: Add this to the configuration
                options.SwaggerDoc("v1", new Info { Title = _configuration["Swagger:Title"], Version = "v1" });
            });

            // this is required for the auth service to sign the jwt token
            services.AddSingleton(_signingCredentials);

            // configuring authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Audience = _authenticationConfiguration.Value.Audience;
    
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = _signingCredentials.Key,
                    ValidIssuer = _authenticationConfiguration.Value.Issuer
                };

                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();

                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";

                        // TODO: This seems to be broken!
                        // TODO: Implement logging
                        return c.Response.WriteAsync(_environment.IsDevelopment() 
                            ? c.Exception.ToString() 
                            : "An error occurred processing your authentication.");
                    }
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.ShowJsonEditor();
             // TODO: add constants for config strings
                c.SwaggerEndpoint(_configuration["Swagger:SwaggerGenEndpoint"], _configuration["Swagger:Title"]);
            });

            app.UseMvc();
        }

        public static void ConfigureStartupServices(WebHostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            // map all configuration sections
            services.Configure<DatabaseConfiguration>(configuration.GetSection(Constants.ConfigurationSections.Database));
            services.Configure<SwaggerConfiguration>(configuration.GetSection(Constants.ConfigurationSections.Swagger));
            services.Configure<AuthenticationConfiguration>(configuration.GetSection(Constants.ConfigurationSections.Authentication));
            services.Configure<LanguageConfiguration>(configuration.GetSection(Constants.ConfigurationSections.Language));
        }
    }
}
