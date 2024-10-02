using CampusVarbergDashBoard.Models;
using CampusVarbergDashBoard.Repository;
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

            // Add services to the container.
            builder.Services.AddControllersWithViews();

			//Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWGNCf1NpRGVGfV5ycEVPal5ZTnJXUiweQnxTdEFjUH1bcHBWQGVcVER/WQ==");


            var app = builder.Build();

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
