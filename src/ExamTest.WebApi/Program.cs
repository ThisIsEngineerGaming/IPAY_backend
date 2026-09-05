using ExamTest.Application.Interfaces.Media;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Application.Services.Media;
using ExamTest.Application.Services.Shop;
using ExamTest.Infastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Firebase Realtime Database (admin, service-account authenticated) - see ExamTest.Infastructure/Firebase.
builder.Services.AddFirebaseInfrastructure(builder.Configuration);

// Application services
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IEpisodeService, EpisodeService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IFilmService, FilmService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
