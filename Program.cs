using System.IO.Compression;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;


//Services

// Specify the dist folder and zip file URL
string normalFolder = Path.Combine(Directory.GetCurrentDirectory());
string distFolder = Path.Combine(Directory.GetCurrentDirectory(), "dist");
string zipFileUrl = "https://storage.googleapis.com/onlinebookbinaries/dist.v1.zip";
string tempZipFile = Path.Combine(Directory.GetCurrentDirectory(), "dist.zip");

// Delete the dist folder if it exists
if (Directory.Exists(distFolder))
    Directory.Delete(distFolder, true);


// Download the zip file
using (var client = new WebClient())
    client.DownloadFile(zipFileUrl, tempZipFile);


// Unzip the downloaded file to the proyect folder
ZipFile.ExtractToDirectory(tempZipFile, normalFolder);

// Delete the temporary zip file
File.Delete(tempZipFile);


var externalAssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "dist/Web.dll");
var assembly = Assembly.LoadFile(externalAssemblyPath);
var builder = WebApplication.CreateBuilder(args);
var mvcBuilder = builder.Services.AddControllersWithViews(options => {
    var filters = assembly.GetTypes().Where(x => x.IsClass && x.Namespace == "Web.Filters" && typeof(IFilterMetadata).IsAssignableFrom(x));
    foreach (var filter in filters) { options.Filters.Add(filter); }
}).AddApplicationPart(assembly);
if (builder.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();
builder.Services.AddHttpClient("client", x => { x.BaseAddress = new Uri($"{builder.Configuration["Settings:BaseAPIURL"]}/"); });
var services = assembly.GetTypes().Where(x => x.IsClass && x.Namespace == "Web.Services");
foreach (var service in services) { builder.Services.AddScoped(service); }

//App
var app = builder.Build();
if (app.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{action=Index}/{id?}",
    defaults: new { controller = "Home" });
app.Run();
