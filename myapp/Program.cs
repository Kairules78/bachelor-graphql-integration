using System.Net;
using System.Net.Http.Json;
using System.Linq;
using HotChocolate;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=/work/data/app.db"));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

var app = builder.Build();

// Ensure DB exists (SQLite file will be created if missing)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "OK – go to /graphql");
app.MapGraphQL("/graphql");

// Print last 50 saved rows as plain text
app.MapGet("/print/countries", async (AppDbContext db) =>
{
    var rows = await db.CountryByNames
        .OrderByDescending(x => x.Id)
        .Take(50)
        .ToListAsync();

    var lines = rows.Select(r =>
        $"{r.FetchedAtUtc:u} | query='{r.NameQuery}' | {r.CommonName} | {r.Capital} | {r.Region} | pop={r.Population}");

    return Results.Text(string.Join("\n", lines), "text/plain");
});

app.Run();

public sealed class Query
{
    private const string BaseUrl = "https://restcountries.com/v3.1";

    public async Task<CountryResult?> CountryByName(
        [Service] IHttpClientFactory httpClientFactory,
        [Service] AppDbContext db,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new GraphQLException("name is required");

        var client = httpClientFactory.CreateClient();

        var url =
            $"{BaseUrl}/name/{Uri.EscapeDataString(name.Trim())}" +
            "?fullText=true" +
            "&fields=name,capital,region,subregion,population,flags";

        using var response = await client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var list = await response.Content
            .ReadFromJsonAsync<List<RestCountryDto>>();

        var dto = list?.FirstOrDefault();
        if (dto is null)
            return null;

        var mapped = Map(dto);

        // Save to SQLite
        db.CountryByNames.Add(new CountryByName
        {
            NameQuery = name.Trim(),
            CommonName = mapped.CommonName,
            OfficialName = mapped.OfficialName,
            Region = mapped.Region,
            Subregion = mapped.Subregion,
            Capital = mapped.Capital,
            Population = mapped.Population,
            FlagPng = mapped.FlagPng,
            FetchedAtUtc = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        return mapped;
    }

    public async Task<List<CountryResult>> AllCountries(
        [Service] IHttpClientFactory httpClientFactory,
        int limit = 25)
    {
        if (limit < 1 || limit > 250)
            throw new GraphQLException("limit must be between 1 and 250");

        var client = httpClientFactory.CreateClient();

        var url =
            $"{BaseUrl}/all" +
            "?fields=name,capital,region,subregion,population,flags";

        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var list = await response.Content
            .ReadFromJsonAsync<List<RestCountryDto>>() ?? new();

        return list
            .Take(limit)
            .Select(Map)
            .ToList();
    }

    // View saved rows (from SQLite)
    public async Task<List<CountryByName>> SavedCountries(
        [Service] AppDbContext db,
        int take = 10)
    {
        if (take < 1 || take > 500)
            throw new GraphQLException("take must be between 1 and 500");

        return await db.CountryByNames
            .OrderByDescending(x => x.Id)
            .Take(take)
            .ToListAsync();
    }

    private static CountryResult Map(RestCountryDto dto) => new()
    {
        CommonName = dto.name?.common,
        OfficialName = dto.name?.official,
        Region = dto.region,
        Subregion = dto.subregion,
        Capital = dto.capital?.FirstOrDefault(),
        Population = dto.population,
        FlagPng = dto.flags?.png
    };
}

public sealed class CountryResult
{
    public string? CommonName { get; set; }
    public string? OfficialName { get; set; }
    public string? Region { get; set; }
    public string? Subregion { get; set; }
    public string? Capital { get; set; }
    public long Population { get; set; }
    public string? FlagPng { get; set; }
}

public sealed class RestCountryDto
{
    public NameDto? name { get; set; }
    public string? region { get; set; }
    public string? subregion { get; set; }
    public List<string>? capital { get; set; }
    public long population { get; set; }
    public FlagsDto? flags { get; set; }

    public sealed class NameDto
    {
        public string? common { get; set; }
        public string? official { get; set; }
    }

    public sealed class FlagsDto
    {
        public string? png { get; set; }
        public string? svg { get; set; }
    }
}

