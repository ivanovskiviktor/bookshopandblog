using BSB.Data;
using BSB.Data.Entity;
using BSB.Repository.Implementation;
using BSB.Repository.Interface;
using BSB.Service.Implementation;
using BSB.Service.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Twilio.Clients;

namespace BSB.Web
{
    public class Startup
    {
        private EmailSettings emailService;

        public Startup(IConfiguration configuration)
        {
            emailService = new EmailSettings();
            Configuration = configuration;
            Configuration.GetSection("EmailSettings").Bind(emailService);

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(
                   Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<BSBUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //DI for Repositories
            services.AddScoped(typeof(IProductRepository), typeof(ProductRepository));
            services.AddScoped(typeof(IOrderRepository), typeof(OrderRepository));
            services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
            services.AddScoped(typeof(IEmailRepository), typeof(EmailRepository));
            services.AddScoped(typeof(IShoppingCartRepository), typeof(ShoppingCartRepository));

            //Sms Configuration
            services.AddHttpClient<ITwilioRestClient, TwilioClient>();

            //Stripe Configuration
            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));

            //DI for Services
            services.AddTransient(typeof(IProductService), typeof(BSB.Service.Implementation.ProductService));
            services.AddTransient(typeof(IOrderService), typeof(BSB.Service.Implementation.OrderService));
            services.AddTransient(typeof(IShoppingCartService), typeof(BSB.Service.Implementation.ShoppingCartService));
            services.AddScoped<IEmailService, EmailService>(email => new EmailService(emailService));

            services.AddScoped<EmailSettings>(es => emailService);

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
