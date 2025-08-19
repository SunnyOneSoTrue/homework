namespace homework.Controllers.SearchParams
{

    public record SearchParams(int page, int pageSize, string? search, string? sortBy, string? sortDir);
}