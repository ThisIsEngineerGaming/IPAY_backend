using IPAY.Application.DTOs.Auth;
using IPAY.Application.Interfaces.Auth;
using IPAY.Application.Interfaces.Media;
using IPAY.Application.Interfaces.Shop;
using IPAY.Application.Mappings.Shop;
using IPAY.Application.Options;
using IPAY.Application.Services.Auth;
using IPAY.Application.Services.Media;
using IPAY.Application.Services.Shop;
using IPAY.Application.Validators.Auth;
using IPAY.Domain.Entities.Users;
using IPAY.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddOpenApi();


builder.Services.AddEndpointsApiExplorer(); // сканує ендпоінти для генерації OpenAPI-документу
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "IPAY", Version = "v1" });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Вставь ТІЛЬКИ токен (без слова Bearer)"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
}); // генерує OpenAPI-документ на основі відсканованих ендпоінтів
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Firebase Realtime Database (admin, service-account authenticated) - see IPAY.Infrastructure/Firebase.
builder.Services.AddFirebaseInfrastructure(builder.Configuration);

// Application services
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IEpisodeService, EpisodeService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IFilmService, FilmService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
builder.Services.AddScoped<IValidator<ChangeUsernameDto>, ChangeUsernameValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
builder.Services.AddScoped<IValidator<StartEmailChangeDto>, StartEmailChangeValidator>();
builder.Services.AddScoped<IValidator<ConfirmEmailChangeDto>, ConfirmEmailChangeValidator>();
builder.Services.AddScoped<IUser<Customer>, CustomerService>();
builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();


// Реєструємо сервіси
builder.Services.AddScoped<IAuthDto<RegisterDto>, RegisterDtoService>();
builder.Services.AddScoped<ILogin, LoginDtoService>();
builder.Services.AddScoped<IGoogleLogin, GoogleLoginService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddSingleton(
    builder.Configuration.GetSection(TwoFactorOptions.SectionName).Get<TwoFactorOptions>() ?? new TwoFactorOptions());
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IJWT,JwtService>();
builder.Services.Configure<OmdbOptions>(
    builder.Configuration.GetSection(OmdbOptions.SectionName));

builder.Services.AddHttpClient<IOmdbService, OmdbService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OmdbOptions>>().Value;

    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});
//Mappers
builder.Services.AddAutoMapper(typeof(ProductMapping));
// JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();
//-Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("SellerOnly", policy =>
        policy.RequireRole("Seller"));

    options.AddPolicy("CustomerOnly", policy =>
    policy.RequireRole("Customer"));

    // если нужно Seller или Admin:
    options.AddPolicy("SellerOrAdmin", policy =>
        policy.RequireRole("Seller", "Admin"));
});


var app = builder.Build();

// Fail-open on purpose (so a missing SMTP config can't lock everyone out), but make it loud.
using (var scope = app.Services.CreateScope())
{
    if (!scope.ServiceProvider.GetRequiredService<ITwoFactorService>().IsEnabled)
    {
        app.Logger.LogWarning(
            "Emailed two-factor codes are OFF (TwoFactor:Enabled is false or the \"Email\" section is not configured). " +
            "Password logins will NOT ask for a code.");
    }
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger(); // генерує /swagger/v1/swagger.json
    app.UseSwaggerUI(); // UI на /swagger

}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
