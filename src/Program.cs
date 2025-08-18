using Microsoft.AspNetCore.Mvc;
using SearchEngine_.utils;
using SearchEngine_.ReadableDocuments;
using SearchEngine_.helpers;
using SearchEngine_.indexing.impl;
using SearchEngine_.services;

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

            // Register custom services
            // Load environment variables from .env if present
            LoadEnvironmentVariables();
            StorageHelper.InitializeStorage();
            builder.Services.AddSingleton<ILinkResolver, LinkResolver>();
            builder.Services.AddSingleton<IReadableDocumentFactory, ReadableDocumentFactory>();
            builder.Services.AddSingleton<IStopWordFilter, StopWordFilter>();
            builder.Services.AddSingleton<IQueryTokenizer, QueryTokenizer>();
            builder.Services.AddSingleton<InvertedIndexService>();
            builder.Services.AddSingleton<SearchEngineService>();
            builder.Services.AddSingleton<InvertedIndexStorageFactory>();

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

        private static void LoadEnvironmentVariables()
        {
            try
            {
                var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (!File.Exists(envPath))
                    return;

                foreach (var line in File.ReadAllLines(envPath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("#"))
                        continue;
                    var separatorIndex = trimmed.IndexOf('=');
                    if (separatorIndex < 0)
                        continue;

                    var key = trimmed.Substring(0, separatorIndex).Trim();
                    var value = trimmed.Substring(separatorIndex + 1).Trim().Trim('"');
                    if (!string.IsNullOrEmpty(key))
                    {
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }
            catch (Exception)
            {
                // Swallow errors silently to avoid startup failure if .env is malformed
            }
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
