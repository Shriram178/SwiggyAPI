using System.Text.Json;
using Foodiee.Models;
using Foodiee.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Foodiee
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddScoped<IClaimsTransformation, KeycloakRoleClaimsTransformer>();
            builder.Services.AddScoped<UserSyncService>();


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://localhost:8080/realms/foodie-realm"; // 👈 Replace with your realm URL
                options.Audience = "account"; // 👈 Replace with your client ID in Keycloak
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username", // or "sub"
                    RoleClaimType = "roles" // 👈 We'll map this below using claims transformation
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("restaurant-owner", policy =>
                    policy.RequireAssertion(context =>
                    {
                        // Check both possible role locations
                        var realmRoles = context.User.FindFirst("realm_access")?.Value;
                        var resourceAccess = context.User.FindFirst("resource_access")?.Value;

                        if (!string.IsNullOrEmpty(resourceAccess))
                        {
                            var resources = JsonSerializer.Deserialize<Dictionary<string, ClientRoles>>(resourceAccess);
                            if (resources?.TryGetValue("foodie-backend", out var clientRoles) == true)
                            {
                                if (clientRoles.Roles.Contains("restaurant-owner"))
                                    return true;
                            }
                        }


                        if (!string.IsNullOrEmpty(realmRoles))
                        {
                            var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmRoles);
                            if (realmAccess?.Roles.Contains("restaurant-owner") == true)
                                return true;
                        }

                        return false;
                    }));
            });

            // Add services to the container.
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Foodiee API", Version = "v1" });

                // 🔐 Add JWT bearer authorization to Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });


            builder.Services.AddDbContext<FoodieeDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });



            var app = builder.Build();

            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseStaticFiles();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
