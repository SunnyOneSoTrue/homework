namespace homework.Core.Entities;

public class SearchHistory
{
    public int Id  { get; set; }

    public string UserId  { get; set; }

    public string QueryJson { get; set; }
    
    public DateTime CreatedAtUtc  { get; set; }
}