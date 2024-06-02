

namespace Domain.Entities;

public class Transformation
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public DateTime Date { get; set; }
    public string FromUserId { get; set; }
    public string ToUserId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public User FromUser { get; set; }

}
