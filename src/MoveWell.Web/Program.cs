using MoveWell.Shared.Globalization.DependencyInjection;
using MoveWell.Shared.Localization.DependencyInjection;
using MoveWell.Web.DependencyInjection;
using MoveWell.Web.Localization;
using MoveWell.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.AddMoveWellWebSerilog();

builder.Services.AddMoveWellMvc();
// Output Persian as real characters instead of &#x...; entities (smaller pages, readable source). Still HTML-safe.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(options =>
    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));
builder.Services.AddMoveWellRequestLocalization();
builder.Services.AddMoveWellJsonLocalization(); // after MVC (keeps the JSON factory)
builder.Services.AddSingleton<UiText>();
builder.Services.AddMoveWellApiClients(builder.Configuration);
builder.Services.AddMoveWellWebAuth(builder.Configuration, builder.Environment);

var app = builder.Build();

try
{
    app.UseMiddleware<CorrelationIdMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/error/{0}");
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // /en prefix → PathBase + request culture. Before static files and routing, so "~/" links keep the language.
    app.UseMoveWellCultures();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Area route: only matches (and only generates links) when area = "Admin".
    app.MapAreaControllerRoute("admin", "Admin", "admin/{controller=Dashboard}/{action=Index}/{id?}");
    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "MoveWell.Web terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
