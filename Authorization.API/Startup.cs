using Authorization.API.Data;
using Authorization.API.Model;
using Authorization.API.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Authorization.API
{
    public class Startup
    {
        private readonly String _swagerVersion;
        private readonly String _swagerTitle;
        private readonly String _swagerDescription;
        private readonly String _swaggerEndpoint;

        private const String _authConnectionString = "AuthDbConnection";
        private const String _authConnectionStringInMemorySQL = "AuthDbConnectionInMemorySQL";
        

        private const String _pathErrorController = "/Error";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _swagerVersion = configuration.GetValue<String>("Swagger:Version");
            _swagerTitle = configuration.GetValue<String>("Swagger:Title");
            _swagerDescription = configuration.GetValue<String>("Swagger:Description");
            _swaggerEndpoint = configuration.GetValue<String>("Swagger:Endpoint");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(_swagerVersion, new Info
                {
                    Version = _swagerVersion,
                    Title = _swagerTitle,
                    Description = _swagerDescription
                });
            });

            services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase("AuthorizationAPI"));

            //Uncomment to use SQL Server
            //services.AddDbContext<DataContext>(options => options.UseSqlServer(Configuration.GetConnectionString(_authConnectionString)));
            

            services.AddMvc();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IHashHelper, HashHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(_swaggerEndpoint, _swagerTitle);
            });

            app.UseExceptionHandler(_pathErrorController);

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
