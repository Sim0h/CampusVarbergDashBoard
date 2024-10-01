using Microsoft.Data.SqlClient;

namespace CampusVarbergDashBoard
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//Connectionstring till databasen
			string connectionString = "Server=projektcampusvarberg.database.windows.net;Database=CampusVarbergDashboardDB;User Id=tcvadmin;Password=campusvarberg1!;";

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				try
				{
					connection.Open();
					Console.WriteLine("Connection to database established");
				}
				catch (Exception e)
				{
					Console.WriteLine("Error: " + e.Message);
				}
			}

			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

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
