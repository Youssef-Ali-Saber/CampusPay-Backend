namespace Domain.Entities;

public class Deposition
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public string? SessionId { get; set; }
    public decimal Balance { get; set; }
    public DateTime Date { get; set; }
}
