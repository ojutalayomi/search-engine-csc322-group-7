using Microsoft.AspNetCore.Mvc;

namespace SearchEngine_
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddControllers(); 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            // Map controllers
            app.MapControllers();

            app.Run();
        }
    }
    
    [ApiController]
    public class HomeController : ControllerBase
    {
        // // GET /
        // [HttpGet("/")]
        // public IActionResult Index()
        // {
        //     return Ok("Welcome to my API!");
        // }
    }
}
