using CampusVarbergDashBoard.Controllers;
using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
using CampusVarbergDashBoard.Services;
using Microsoft.Data.SqlClient;

namespace CampusVarbergDashBoard
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);


            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddScoped<IApplicantRepository>(provider => new ApplicantRepository(connectionString));
            builder.Services.AddScoped<GeoCodingService>();

            builder.Services.AddScoped<ApplicantService>();

            builder.Services.AddScoped<IYearRepository>(provider => new YearRepository(connectionString));



            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2UlhhQlVMfV5DQmJPYVF2R2dJfFR0d19DY0wgOX1dQl9nSH5ScUVmWn1ac3ZUR2A=");


            var app = builder.Build();

            //Denna kontrollerar om det finns tomma long och lat och
            //uppdaterar ifall det �r s�, men program.cs beh�ver vara async f�r det

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;

            //    try
            //    {
            //        var applicantService = services.GetRequiredService<ApplicantService>();
            //        await applicantService.UpdateApplicantsCoordinatesAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Ett fel uppstod vid uppdatering av koordinater: {ex.Message}");
            //    }
            //}

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

        }
    }
}
