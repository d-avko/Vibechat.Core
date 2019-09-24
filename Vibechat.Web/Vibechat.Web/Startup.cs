using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.BusinessLogic.Middleware;
using Vibechat.DataLayer;
using Vibechat.DataLayer.Repositories;
using Vibechat.SignalR.Hubs;

namespace Vibechat.Web
{
    public class Startup
    {
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration configuration, 
            IWebHostEnvironment environment)
        { 
            Configuration = configuration;
            this.environment = environment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (environment.IsStaging() || environment.IsDevelopment())
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(Configuration["ConnectionStrings:Development"]));
            }
            else if(environment.IsProduction())
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(Configuration["ConnectionStrings:DefaultConnection"]));
            }

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddEntityFrameworkNpgsql();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddAuthentication(options =>
                {
                    // Identity made Cookie authentication the default.
                    // However, we want JWT Bearer Auth to be the default.
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // Configure JWT Bearer Auth to expect our security key

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            LifetimeValidator =
                                (before, expires, token, param) => expires > DateTime.UtcNow,
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"])),
                            ValidateIssuerSigningKey = true,
                        };


                    // We have to hook the OnMessageReceived event in order to
                    // allow the JWT authentication handler to read the access
                    // token from the query string when a WebSocket or 
                    // Server-Sent Events request comes in.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/chat"))
                                // Read the token out of the query string
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            
            services.AddMvc(x => x.EnableEndpointRouting = false)
                .AddNewtonsoftJson()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vibechat API", Version = "v1" });
            });

            services.AddSignalR();//.AddNewtonsoftJsonProtocol();
            
            services.AddHttpClient<HttpClient>(options =>
            {
                options.BaseAddress = new Uri(Configuration["FileServer:Url"]);
            });

            services.AddDefaultServices();

            services.AddDefaultRepositories();

            services.AddBusinessLogic();
            
            services.AddDefaultMiddleware();
            
            services.AddAuthServices();
            
            services.AddSpaStaticFiles(opts => opts.RootPath = "ClientApp/dist");

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            
            app.UseResponseCompression();
            
            app.UseRequestLocalization();

            app.UseRewriter(new RewriteOptions().AddRewrite(".*/api(.*)", "/api$1", 
                false));

            app.UseStaticFiles();

            app.UseAuthentication();
            
            app.UseMiddleware<UserStatusMiddleware>();

            app.UseRouting();
            
            app.UseEndpoints(builder => builder.MapHub<ChatsHub>("/hubs/chat"));

            app.UseCors("AllowAllOrigins");

            app.UseMvc();

            app.MapWhen(context => context.IsSpaPath(), builder =>
            {
                builder.MapWhen(context => context.IsEnglishRequest(builder.ApplicationServices.GetService<IServiceProvider>()),
                    config =>
                    {
                        config.UseSpaStaticFiles(new StaticFileOptions()
                        {
                            FileProvider = new PhysicalFileProvider(
                                Path.Combine(Directory.GetCurrentDirectory(), "ClientApp/dist")),
                            ServeUnknownFileTypes = true
                        });

                        config.UseSpa(spa =>
                        {
                            // To learn more about options for serving an Angular SPA from ASP.NET Core,
                            // see https://go.microsoft.com/fwlink/?linkid=864501

                            spa.Options.SourcePath = "ClientApp";
                            spa.Options.DefaultPage = "/en/index.html";
                            spa.Options.StartupTimeout = TimeSpan.FromMinutes(5);
                            if (env.IsDevelopment())
                            {
                                spa.UseAngularCliServer("start");
                            }
                        });
                    }
                ).MapWhen(context => context.IsRussianRequest(builder.ApplicationServices.GetService<IServiceProvider>()), 
                    config =>
                {
                    config.UseSpaStaticFiles(new StaticFileOptions()
                    {
                        FileProvider = new PhysicalFileProvider(
                            Path.Combine(Directory.GetCurrentDirectory(), "ClientApp/dist")),
                        ServeUnknownFileTypes = true
                    });

                    config.UseSpa(spa =>
                    {
                        // To learn more about options for serving an Angular SPA from ASP.NET Core,
                        // see https://go.microsoft.com/fwlink/?linkid=864501

                        spa.Options.SourcePath = "ClientApp";
                        spa.Options.DefaultPage = "/ru/index.html";
                        spa.Options.StartupTimeout = TimeSpan.FromMinutes(5);
                        if (env.IsDevelopment())
                        {
                            spa.UseAngularCliServer("start");
                        }
                    });
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vibechat API V1");
                c.RoutePrefix = string.Empty;
            });

            if (environment.IsProduction())
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(Configuration["Credentials:Firebase:Path:Prod"])
                });   
            }
            else
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(Configuration["Credentials:Firebase:Path:Dev"])
                });  
            }

            #region Db Migration on startup

            var db = serviceProvider.GetService<ApplicationDbContext>();

            db.Database.Migrate();

            #endregion

            #region Set all users status to offline and delete connections

            var users = serviceProvider.GetService<UserManager<AppUser>>();

            foreach (var user in users.Users)
            {
                user.IsOnline = false;
            }

            var connections = serviceProvider.GetService<IConnectionsRepository>();

            connections.ClearAsync().GetAwaiter().GetResult();

            #endregion

            #region Create admin account, if not created.

            users.SeedAdminAccount(serviceProvider.GetService<IJwtTokenGenerator>());
            #endregion
        }
    }
}