public sealed class CountryByName
{
    public int Id { get; set; }

    public string NameQuery { get; set; } = default!;   // hva brukeren søkte etter
    public string? CommonName { get; set; }
    public string? OfficialName { get; set; }
    public string? Region { get; set; }
    public string? Subregion { get; set; }
    public string? Capital { get; set; }
    public long Population { get; set; }
    public string? FlagPng { get; set; }

    public DateTimeOffset FetchedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

