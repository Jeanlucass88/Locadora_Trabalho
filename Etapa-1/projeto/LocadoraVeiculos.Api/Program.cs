using LocadoraVeiculos.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<LocadoraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocadoraDb")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

