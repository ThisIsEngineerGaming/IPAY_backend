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
