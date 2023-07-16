using Microsoft.EntityFrameworkCore;
using ShortUrl.BackEnd.Data;
using ShortUrl.BackEnd.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add services for Sql Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UrlDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();
app.UseCors(policy => policy.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/shorturl", async (UrlDto url, UrlDbContext db, HttpContext ctx) =>
{
    //Validate the input URL
    if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var inputUrl))
        return Results.BadRequest("Invalid url has been provided");

    //Create a short version of the provided Url
    var random = new Random();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@az";
    var randomStr = new string(Enumerable.Repeat(chars, 8)
        .Select(x => x[random.Next(x.Length)]).ToArray());
    //Mapping the short Url with long Url
    var sUrl = new UrlManagement()
    {
        Url = url.Url,
        ShortUrl = randomStr
    };

    //Saving the mapping to database
    db.Urls.Add(sUrl);
    await db.SaveChangesAsync();

    //Construct Url
    var result = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{sUrl.ShortUrl}";

    return Results.Ok(new UrlShortResponseDto()
    {
        Url = result
    });
});

app.MapFallback(async (UrlDbContext db, HttpContext ctx) =>
{
    var path = ctx.Request.Path.ToUriComponent().Trim('/');
    var urlMatch = await db.Urls.FirstOrDefaultAsync(x => x.ShortUrl.Trim() == path.Trim());

    if (urlMatch == null)
    { return Results.BadRequest("Invalid request"); }

    return Results.Redirect(urlMatch.Url);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
