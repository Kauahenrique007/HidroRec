using HidroRec.Backend.Configurations;
using HidroRec.Backend.Infrastructure.Data;
using HidroRec.Backend.Middlewares;
using Microsoft.Extensions.FileProviders;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/hidrorec-.log", rollingInterval: RollingInterval.Day);
});

builder.Services
    .AddHidroRecOptions(builder.Configuration)
    .AddHidroRecData(builder.Configuration)
    .AddHidroRecSecurity(builder.Configuration)
    .AddHidroRecApplication()
    .AddHidroRecSwagger();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("FrontendDevelopment");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HidroRec API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

var frontendCandidates = new[]
{
    Path.Combine(builder.Environment.ContentRootPath, "frontend"),
    Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "frontend"))
};

var frontendPath = frontendCandidates.FirstOrDefault(Directory.Exists) ?? frontendCandidates[0];
if (Directory.Exists(frontendPath))
{
    var fileProvider = new PhysicalFileProvider(frontendPath);
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider,
        DefaultFileNames = new List<string> { "index.html" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider,
        RequestPath = string.Empty
    });
}

var uploadsPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["FileStorage:UploadsPath"] ?? "uploads"));
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", async context =>
{
    var indexPath = Path.Combine(frontendPath, "index.html");
    if (File.Exists(indexPath))
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(indexPath);
        return;
    }

    context.Response.Redirect("/swagger");
});

app.MapFallback(async context =>
{
    var requestPath = context.Request.Path.Value ?? string.Empty;
    var candidate = requestPath.TrimStart('/');
    var filePath = Path.Combine(frontendPath, candidate);

    if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(filePath))
    {
        await context.Response.SendFileAsync(filePath);
        return;
    }

    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await context.Response.WriteAsJsonAsync(new
    {
        success = false,
        message = "Recurso nao encontrado."
    });
});

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
