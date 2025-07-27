using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using api.Hubs;
using api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
        options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
    });

// ✅ Thêm SignalR
builder.Services.AddSignalR();

// ✅ Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .SetIsOriginAllowed(_ => true) // CORS an toàn cho dev
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 👈 Rất quan trọng cho SignalR
    });
});

builder.Services.AddSingleton<FirebaseMessagingService>();

builder.Services.AddSwaggerGen();
builder.Services.AddSession();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetApplicationName("IeltsApp");

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("apiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5035/"); // Thay bằng URL API của bạn
});
builder.Services.AddHttpClient();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

// ✅ Kích hoạt CORS
app.UseCors("AllowAll");
app.MapHub<ChatHub>("/chatHub");
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();


app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapGet("/", context => {
        context.Response.Redirect("/User");
        return Task.CompletedTask;
    });
});

app.Run();
