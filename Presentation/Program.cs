using AggregateVersions.Application.Services;
using AggregateVersions.Domain.Interfaces;
using AggregateVersions.Infrastructure.Data;
using AggregateVersions.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var en = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
en = !string.IsNullOrEmpty(en) ? "Development." : en;

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile($"appsettings.{en}json", optional: false, reloadOnChange: true);

builder.Services.AddTransient<IProjectsService, ProjectsService>();
builder.Services.AddScoped<IProjectsRepository, ProjectsRepository>();

builder.Services.AddTransient<IOperationsService, OperationsServices>();
builder.Services.AddScoped<IOperationsRepository, OperationsRepository>();

builder.Services.AddTransient<IApplicationsService, ApplicationsService>();
builder.Services.AddScoped<IApplicationsRepository, ApplicationsRepository>();

builder.Services.AddTransient<IDataBasesService, DataBasesService>();
builder.Services.AddScoped<IDataBasesRepository, DataBasesRepository>();

builder.Services.AddTransient<IAccessesService, AccessesService>();
builder.Services.AddScoped<IAccessesRepository, AccessesRepository>();


builder.Services.AddDbContext<OperationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


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
    pattern: "{controller=Versions}/{action=Index}/{id?}");

app.Run();
