using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApplication.Models;
using WebApplication.Services;
using SignalRChat.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WebApplication
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
            services.Configure<ChatDatabaseSettings>(
                Configuration.GetSection(nameof(ChatDatabaseSettings)));

            services.AddSingleton<IChatDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<ChatDatabaseSettings>>().Value);

            services.AddSingleton<ChatsService>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:3001", "http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            //UserDb MSSQL Server Connection
            services.AddDbContext<UserDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Scoped);
            //Configure Identity
            services
                .AddIdentity<User, IdentityRole>(
                    options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequiredLength = 1;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                    })
                .AddEntityFrameworkStores<UserDbContext>(); // Tell Identity which EF DbContext to use

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();
            
            services.AddSingleton<ChatHub>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseMvc();

            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chathub");
            });
        }
    }
}
