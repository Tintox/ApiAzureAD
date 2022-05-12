using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace APIExterna
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

            /*
              * Agregar Metodo de Authentificacion para API Externa
              * 
              */

            ////Ejemplo Metodo Utilizado para demonio Cliente-API Externo( #Region )
            #region 
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                      .AddJwtBearer(opt =>
                      {
                          opt.Audience = Configuration["AAD:ResourceId"];
                          opt.Authority = $"{Configuration["AAD:Instance"]}{Configuration["AAD:TenantId"]}";
                      });
            #endregion


            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            ////Utilizada para demonio Cliente-API Externo ( #Region )
            #region
             app.UseAuthentication();
            #endregion
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
