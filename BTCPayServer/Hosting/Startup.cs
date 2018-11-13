using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System;
using Microsoft.Extensions.DependencyInjection;
using BTCPayServer.Filters;
using BTCPayServer.Models;
using Microsoft.AspNetCore.Identity;
using BTCPayServer.Data;
using Microsoft.Extensions.Logging;
using Hangfire;
using BTCPayServer.Logging;
using Microsoft.Extensions.Configuration;
using Hangfire.AspNetCore;
using BTCPayServer.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Hangfire.Dashboard;
using Hangfire.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Cryptography;
using AspNet.Security.OpenIdConnect.Primitives;
using BTCPayServer.Controllers.NewApi;
using BTCPayServer.Security;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using NETCore.Encrypt.Extensions.Internal;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BTCPayServer.Hosting
{
    public class Startup
    {
        class NeedRole : IDashboardAuthorizationFilter
        {
            string _Role;
            public NeedRole(string role)
            {
                _Role = role;
            }
            public bool Authorize([NotNull] DashboardContext context)
            {
                return context.GetHttpContext().User.IsInRole(_Role);
            }
        }
        public Startup(IConfiguration conf, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Configuration = conf;
            _Env = env;
            LoggerFactory = loggerFactory;
        }
        IHostingEnvironment _Env;
        public IConfiguration Configuration
        {
            get; set;
        }
        public ILoggerFactory LoggerFactory { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Logs.Configure(LoggerFactory);
            services.ConfigureBTCPayServer(Configuration);
            services.AddMemoryCache();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();      
            
            // Register the OpenIddict services.
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and entities.
                    options.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>();
                })

                .AddServer(options =>
                {
                    // Register the ASP.NET Core MVC binder used by OpenIddict.
                    // Note: if you don't call this method, you won't be able to
                    // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                    options.UseMvc();

                    // Enable the token endpoint (required to use the password flow).
                    options.EnableTokenEndpoint("/connect/token");
                    options.EnableAuthorizationEndpoint("/connect/authorize");
                    options.EnableAuthorizationEndpoint("/connect/logout");

                    // Allow client applications various flows
                    // with the exception of Client Credentials flow as we cannot identify
                    // a user through that grant type
                    options.AllowImplicitFlow();
                    options.AllowRefreshTokenFlow();
                    options.AllowPasswordFlow();
                    options.AllowAuthorizationCodeFlow();
                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();

                    // Accept token requests that don't specify a client_id. 
                    options.AcceptAnonymousClients();
                    options.DisableScopeValidation();

                    options.UseJsonWebTokens();

                    options.ConfigureSigningKey(Configuration);
                });

            services.AddBTCPayServer(Configuration);
            services.AddMvc(o =>
            {
                o.Filters.Add(new XFrameOptionsAttribute("DENY"));
                o.Filters.Add(new XContentTypeOptionsAttribute("nosniff"));
                o.Filters.Add(new XXSSProtectionAttribute());
                o.Filters.Add(new ReferrerPolicyAttribute("same-origin"));
                //o.Filters.Add(new ContentSecurityPolicyAttribute()
                //{
                //    FontSrc = "'self' https://fonts.gstatic.com/",
                //    ImgSrc = "'self' data:",
                //    DefaultSrc = "'none'",
                //    StyleSrc = "'self' 'unsafe-inline'",
                //    ScriptSrc = "'self' 'unsafe-inline'"
                //});
            });
            services.TryAddScoped<ContentSecurityPolicies>();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 7;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequireUppercase = false;            
                // Configure Identity to use the same JWT claims as OpenIddict instead
                // of the legacy WS-Federation claims it uses by default (ClaimTypes),
                // which saves you from doing the mapping in your authorization controller.
                
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });

            services.AddHangfire((o) =>
            {
                var scope = AspNetCoreJobActivator.Current.BeginScope(null);
                var options = (ApplicationDbContextFactory)scope.Resolve(typeof(ApplicationDbContextFactory));
                options.ConfigureHangfireBuilder(o);
            });
            services.AddCors(o =>
            {
                o.AddPolicy("BitpayAPI", b =>
                {
                    b.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                });
            });
            services.AddSwaggerGen(c =>  
            {  
                c.DocInclusionPredicate((s, description) =>
                {
                    if (description.TryGetMethodInfo(out var methodInfo))
                    {
                        
                        return methodInfo.CustomAttributes.Any(data =>
                                   data.AttributeType == typeof(IncludeInOpenApiDocs)) ||
                               methodInfo.DeclaringType.CustomAttributes.Any(data =>
                                   data.AttributeType == typeof(IncludeInOpenApiDocs));
                    }

                    return false;
                });
                c.SwaggerDoc("v0.1", new Info { Title = "BtcPayServer Preview API", Version = "v0.1" });  
                c.DescribeAllEnumsAsStrings();
                // JWT-token authentication by password
                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "password",
                    TokenUrl = Path.Combine(Configuration.GetExternalUri()?.ToString()?? "http://127.0.0.1:14142/", "/connect/token"),
                    // Optional scopes
                    //Scopes = new Dictionary<string, string>
                    //{
                    //    { "api-name", "my api" },
                    //}
                });
            }); 

            // If the HTTPS certificate path is not set this logic will NOT be used and the default Kestrel binding logic will be.
            string httpsCertificateFilePath = Configuration.GetOrDefault<string>("HttpsCertificateFilePath", null);
            bool useDefaultCertificate = Configuration.GetOrDefault<bool>("HttpsUseDefaultCertificate", false);
            bool hasCertPath = !String.IsNullOrEmpty(httpsCertificateFilePath);
            if (hasCertPath || useDefaultCertificate)
            {
                var bindAddress = Configuration.GetOrDefault<IPAddress>("bind", IPAddress.Any);
                int bindPort = Configuration.GetOrDefault<int>("port", 443);

                services.Configure<KestrelServerOptions>(kestrel =>
                {
                    if (hasCertPath && !File.Exists(httpsCertificateFilePath))
                    {
                        // Note that by design this is a fatal error condition that will cause the process to exit.
                        throw new ConfigException($"The https certificate file could not be found at {httpsCertificateFilePath}.");
                    }
                    if(hasCertPath && useDefaultCertificate)
                    {
                        throw new ConfigException($"Conflicting settings: if HttpsUseDefaultCertificate is true, HttpsCertificateFilePath should not be used");
                    }

                    kestrel.Listen(bindAddress, bindPort, l =>
                    {
                        if (hasCertPath)
                        {
                            Logs.Configuration.LogInformation($"Using HTTPS with the certificate located in {httpsCertificateFilePath}.");
                            l.UseHttps(httpsCertificateFilePath, Configuration.GetOrDefault<string>("HttpsCertificateFilePassword", null));
                        }
                        else
                        {
                            Logs.Configuration.LogInformation($"Using HTTPS with the default certificate");
                            l.UseHttps();
                        }
                    });
                });
            }
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IServiceProvider prov,
            BTCPayServerOptions options,
            ILoggerFactory loggerFactory)
        {
            Logs.Configuration.LogInformation($"Root Path: {options.RootPath}");
            if (options.RootPath.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                ConfigureCore(app, env, prov, loggerFactory, options);
            }
            else
            {
                app.Map(options.RootPath, appChild =>
                {
                    ConfigureCore(appChild, env, prov, loggerFactory, options);
                });
            }
        }

        private static void ConfigureCore(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider prov, ILoggerFactory loggerFactory, BTCPayServerOptions options)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UsePayServer();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                AppPath = options.GetRootUri(),
                Authorization = new[] { new NeedRole(Roles.ServerAdmin) }
            });
            app.UseWebSockets();
            app.UseStatusCodePages();
            
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "BtcPayServer Preview API 0.1");
                
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
