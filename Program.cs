using DinamikCvSitesi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Development mode flag for easy reference
bool isDevelopment = builder.Environment.IsDevelopment();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Add global filters for security in all environments
    // Only apply antiforgery token global validation in production
    if (!isDevelopment)
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    }
});

// Add Entity Framework DbContext
builder.Services.AddDbContext<DinamikCvSitesi.Models.CVContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure cookie and session policies based on environment
var cookieSecurePolicy = isDevelopment 
    ? Microsoft.AspNetCore.Http.CookieSecurePolicy.None 
    : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;

var cookieSameSiteMode = isDevelopment
    ? Microsoft.AspNetCore.Http.SameSiteMode.Lax
    : Microsoft.AspNetCore.Http.SameSiteMode.Strict;

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.Name = ".DinamikCv.Session";
    options.Cookie.SameSite = cookieSameSiteMode;
});

// Add cookie authentication
builder.Services.AddAuthentication("DinamikCvSitesiCookie")
    .AddCookie("DinamikCvSitesiCookie", options =>
    {
        options.Cookie.Name = ".DinamikCv.Auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/Admin/Login";
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
        options.Cookie.SameSite = cookieSameSiteMode;
    });

// Configure antiforgery - completely disable SSL requirements in development
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = ".DinamikCv.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = cookieSameSiteMode;
    
    // Important: for development, completely disable SSL requirement for antiforgery
    if (isDevelopment)
    {
        options.SuppressXFrameOptionsHeader = true;
    }
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CVContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (isDevelopment)
{
    // Development environment configuration
    app.UseDeveloperExceptionPage();
    
    // Basic security headers for development - remove HTTPS requirements
    app.Use(async (context, next) =>
    {
        // No X-Frame-Options header in development to prevent issues with antiforgery tokens
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        await next();
    });
}
else
{
    // Production environment configuration
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
    
    // Enhanced security headers for production
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' https://cdn.jsdelivr.net; img-src 'self' data:;";
        await next();
    });
}

// Forward headers if behind a proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Static files
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=86400";
    }
});

app.UseRouting();

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Use session middleware
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
