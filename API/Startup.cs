using API.Data;
using API.Entities;
using API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {
        // Construtor: inject configuration from appsettings.json (e.g. DB connection string)
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container. (Dependency Injection Container)
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
            // StoreContext is derived from EF class DbContext
            services.AddDbContext<StoreContext>(opt =>
            {
                // pass in option for SQLite connection string
                opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
            // Add CORS
            services.AddCors();
            // Add identity configuration
            services.AddIdentityCore<User>(opt =>
            {
                // don't allow duplicate email
                opt.User.RequireUniqueEmail = true;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<StoreContext>(); // add all identity tables (AspNetRoles, AspNetUserLogins etc.)
            // add auth
            services.AddAuthentication();
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline. (Middleware)
        // The ORDER of middlewares is very important
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // use custom exception middleware
            app.UseMiddleware<ExceptionMiddleware>();

            if (env.IsDevelopment())
            {
                // app.UseDeveloperExceptionPage(); // use custom exception middleware instead of this one
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            // app.UseHttpsRedirection(); // not using HTTPS in dev mode

            app.UseRouting();
            // CORS middleware must come after UseRouting()
            app.UseCors(opt =>
            {
                opt
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials() // pass cookies to/from client
                    .WithOrigins("http://localhost:3000");
            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
