
using Investly.DAL.Entities;
using Investly.DAL.Repos;
using Investly.DAL.Repos.IRepos;
using Investly.DAL.Seeding;
using Investly.PL.BL;
using Investly.PL.General;
using Investly.PL.General.Services;
using Investly.PL.General.Services.IServices;
using Investly.PL.IBL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Investly.PL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var config = builder.Configuration.GetSection("Jwt");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Issuer"],
                    ValidAudience = config["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(config["Key"]))
                };
            });

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

           
            #region General services registeration
            builder.Services.AddScoped<IJWTService, JWTService>();
            builder.Services.AddScoped(typeof(IQueryService<>), typeof(QueryService<>));
            builder.Services.AddScoped(typeof(IRepo<>), typeof(Repo<>));
            #endregion

            #region Unit of work  registeration
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            #region Business services registeration
            builder.Services.AddScoped<IInvestorService, InvestorService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IGovernementService,GovernmentService>();
            builder.Services.AddScoped<IFounderService, FounderService>();
            builder.Services.AddScoped<IInvestorContactRequestService, InvestorContactRequestService>();
            #endregion

            var app = builder.Build();


            #region DataSeeding
            using (var scope = app.Services.CreateScope())
            {
                var services= scope.ServiceProvider;
                var dbContext = services.GetRequiredService<AppDbContext>();
             
                    // Ensure the database is created and apply migrations
                   // dbContext.Database.Migrate();
                    var seeder = new DataSeeding(dbContext);
                    // Seed the database with initial data
                   //seeder.SuperAdminSeed();
                  // seeder.GovernmentCitiesSeed();
                
            }
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.UseCors("AllowAllOrigins");

            app.Run();
        }
    }
}
