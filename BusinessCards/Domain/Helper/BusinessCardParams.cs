using Domain.Enum;

namespace Domain.Helper;

public class BusinessCardParams
{
    public string? name { get; set; }
    public string? email { get; set; }
    public string? phone { get; set; }
    public Gender? gender { get; set; }

    public DateTime? dob { get; set; }        
    public DateTime? dobFrom { get; set; }     
    public DateTime? dobTo { get; set; }       

    private int _pageSize = 3;
    public int MaxPageSize { get; set; } = 50;
    public int pageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 3 : Math.Min(value, MaxPageSize);
    }
    public int PageNumber { get; set; } = 1;

    public string? sortBy { get; set; } = "name"; 
    public string? sortDir { get; set; } = "asc"; 
}
