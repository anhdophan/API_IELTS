using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
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
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();


app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapGet("/", context => {
    context.Response.Redirect("/Admin/Login_Admin");
    return Task.CompletedTask;
    });
});

app.Run();
