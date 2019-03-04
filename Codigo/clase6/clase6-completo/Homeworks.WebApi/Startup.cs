using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


using Homeworks.BusinessLogic;
using Homeworks.BusinessLogic.Interface;
using Homeworks.DataAccess;
using Homeworks.DataAccess.Interface;
using Homeworks.Domain;
using Homeworks.ServiceFactory;

namespace Homeworks.WebApi
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<DbContext, HomeworksContext>(
               o => o.UseSqlServer(@"Server=127.0.0.1,1401;Database=HomeworksDB;User Id=sa;Password=YourStrong!Passw0rd;")
            );

            // services.AddDbContext<DbContext, HomeworksContext>(
            //     options => options.UseInMemoryDatabase("HomeworksDB"));

            services.AddLogic<IUsersLogic>();
            services.AddLogic<IHomeworksLogic>();
            services.AddLogic<IExerciseLogic>();
            services.AddLogic<ISessionsLogic>();

            services.AddRepository<IRepository<Homework>>();
            services.AddRepository<IRepository<Session>>();
            services.AddRepository(typeof(IRepository<>));
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

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
