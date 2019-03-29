using AutoMapper;
using OpenMSM.Web.Mapping;
using OpenMSM.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security;
using System.Security.Cryptography;
using OpenMSM.Web.Hubs;
using OpenMSM.Web.Middleware;
using Microsoft.Extensions.Logging;
using SoapCore;
using System.ServiceModel;

namespace OpenMSM.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<OpenMSM.Data.AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient("DefaultHttpClient", client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "OpenMSM Service Provider");
            });

            services.AddScoped<AdminHub>();
            services.AddScoped<ChannelManagementService>();
            services.AddScoped<ConsumerPublicationService>();
            services.AddScoped<ProviderPublicationService>();
            services.AddScoped<ConsumerRequestService>();
            services.AddScoped<ProviderRequestService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<PingService>();
            services.AddScoped<IMessageFilter, SOAPHeaderHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSoapCore();

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var mapperConfig = new MapperConfiguration(m =>
            {
                m.AddProfile(new MappingProfile());
            });

            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddSignalR();
            services.AddCors();

            services.AddLogging(loggingBuilder => {
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors(builder => builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSoapEndpoint<PingService>("/PingService.svc", binding: new BasicHttpBinding(), SoapSerializer.DataContractSerializer);
            app.UseSoapEndpoint<ChannelManagementService>("/ChannelManagementService.asmx", binding: new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            app.UseSoapEndpoint<ConsumerPublicationService>("/ConsumerPublicationService.asmx", binding: new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            app.UseSoapEndpoint<ConsumerRequestService>("/ConsumerRequestService.asmx", binding: new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            app.UseSoapEndpoint<NotificationService>("/NotificationService.asmx", binding: new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            app.UseSoapEndpoint<ProviderPublicationService>("/ProviderPublicationService.asmx", binding: new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            app.UseSoapEndpoint<ProviderRequestService>("/ProviderRequestService.asmx", binding: new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            app.UseRequestResponseLogging();
            app.UseMvc();
            
            app.UseSignalR(routes => {
                routes.MapHub<OpenMSM.Web.Hubs.AdminHub>("/admin/hub");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });


            ConfigureApplicationSalt();
        }

        public void ConfigureApplicationSalt()
        {
            var salt = Environment.GetEnvironmentVariable(OpenMSM.Web.Services.ServiceBase.TokenSaltEV, EnvironmentVariableTarget.Machine);
            if (salt == null)
            {
                var saltBytes = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                salt = Convert.ToBase64String(saltBytes);
                Environment.SetEnvironmentVariable(OpenMSM.Web.Services.ServiceBase.TokenSaltEV, salt, EnvironmentVariableTarget.Machine);
            }
        }
    }
}
