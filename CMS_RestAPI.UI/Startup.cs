using AutoMapper;
using CMS_RestAPI.DataAccessLayer.Context;
using CMS_RestAPI.DataAccessLayer.Repositories.Concrete.EntityTypeRepositories;
using CMS_RestAPI.DataAccessLayer.Repositories.Interfaces.EntityTypeRepositories;
using CMS_RestAPI.UI.Mapper;
using CMS_RestAPI.UI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CMS_RestAPI.UI
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
            services.AddControllers();
           
            services.AddRouting(x => x.LowercaseUrls = true);

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAutoMapper(typeof(AppUserMapper));
            services.AddAutoMapper(typeof(CategoryMapper));
            services.AddAutoMapper(typeof(PageMapper));
            services.AddAutoMapper(typeof(ProductMapper));

            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("CMS API", new OpenApiInfo()
                {
                    Title = "CMS API",
                    Version = "V.1",
                    Description = "CMS API",
                    Contact = new OpenApiContact()
                    {
                        Email = "samettteraslan@gmail.com",
                        Name = "Samet ERASLAN",
                        Url = new Uri("https://github.com/sameteeraslann")
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "MIT Licance",
                        Url = new Uri("https://github.com/sameteeraslann")
                    }
                });

                var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommnetFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);
                options.IncludeXmlComments(xmlCommnetFullPath);
            });

            // API'�n sahib oldu�u yetenekler yani Controller i�erisindeki Action Metodlar�m�za yazd���m�z summary yani �zet bilgilerin Swagger UI arac�nda g�z�kmesi i�in yap�lan bir konfigurasyon.

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.SecretKey);
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
            //CMS Rest API farkl� bir domainde, API'ye request atan web siteside haliyle farkl� bir domainde olacakt�r. Farkl� origin'lerde kaynaklarda bulunan bu web platformlar�n sa�l�kl� bir �ekilde ileti�im kurmas� i�in API'ye request atan web sitelerini tan�tmam�z gerekmektedir. Bunun i�in hiyerar�ik olarak bulunan "UseCors" middleware kullan�lacakt�r. �zellikle veritaban�na varl�k insert etmek istedi�imizde yada var olan bir val�k �zerinde de�i�iklik yapmak istedi�imizde ataca��m�z requestlerin ba�ar�l� olmas� i�in a�a��da i�lemin yap�lmas� gerekmektedir.
            app.UseCors(options => options
                .AllowAnyOrigin() // Request atan web projesinin bulundu�u domain bilgisi, bu method i�erisinde herhangi bir domain bilgisi verilmezse world wide web i�erisinde herhangi bir alan ad�na sahip web sitesi bize request atabilir.
                .AllowAnyMethod() // Hangi methodlara izin verildi�i
                .AllowAnyHeader() // Hangi request header'lar�na izin verildi�i ad�m ad�m burdaki middleware'da titiz bir �ekilde belirlenir.

            //Standart .Net i�erisinde de ayn� mant�k bulunmaktad�r. Standart .Net i�erisinde Web Config i�erisinde genel ayarlar yap�labilindi�i gibi Controller i�erisine girerek action methodlara attribute olarakta bu origin bilgileri verile bilir. Method bu ayarlar verilebilir.

            //Asp .Net Core i�in [EnableCors] attribute vas�tas�yla yap�l�r.

            //Bu global ayarlar controller baz�nda ve action method baz�nda yap�lmaktad�r.
            );

            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/CMS API/swagger.json", "CMS API"); });

            app.UseAuthentication();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
