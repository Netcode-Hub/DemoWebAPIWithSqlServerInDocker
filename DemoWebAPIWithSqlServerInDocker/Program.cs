using Microsoft.EntityFrameworkCore;
using DemoWebAPIWithSqlServerInDocker;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
var app = builder.Build();
MigrationService.InitializeMigration(app);

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


//app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();

app.MapControllers();

app.MapProductEndpoints();

app.Run();
