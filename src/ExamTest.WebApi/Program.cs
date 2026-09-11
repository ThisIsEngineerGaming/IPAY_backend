using ExamTest.Application.DTOs.Auth;
using ExamTest.Application.Interfaces.Auth;
using ExamTest.Application.Interfaces.Media;
using ExamTest.Application.Interfaces.Shop;
using ExamTest.Application.Options;
using ExamTest.Application.Services.Auth;
using ExamTest.Application.Services.Media;
using ExamTest.Application.Services.Shop;
using ExamTest.Application.Validators.Auth;
using ExamTest.Domain.Entities.Auth;
using ExamTest.Infastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddOpenApi();


builder.Services.AddEndpointsApiExplorer(); // сканує ендпоінти для генерації OpenAPI-документу
builder.Services.AddSwaggerGen(); // генерує OpenAPI-документ на основі відсканованих ендпоінтів
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Firebase Realtime Database (admin, service-account authenticated) - see ExamTest.Infastructure/Firebase.
builder.Services.AddFirebaseInfrastructure(builder.Configuration);

// Application services
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IEpisodeService, EpisodeService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IFilmService, FilmService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
builder.Services.AddScoped<IUser<Seller>, SellerService>();

// Реєструємо сервіси
builder.Services.AddScoped<IAuth<RegisterDto>, RegisterService>();
builder.Services.AddScoped<IAuth<LoginDto>, LoginService>();
builder.Services.Configure<OmdbOptions>(
    builder.Configuration.GetSection(OmdbOptions.SectionName));

builder.Services.AddHttpClient<IOmdbService, OmdbService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OmdbOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger(); // генерує /swagger/v1/swagger.json
    app.UseSwaggerUI(); // UI на /swagger

}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
