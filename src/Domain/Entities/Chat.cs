namespace Domain.Entities;

public class Chat
{

    public int Id { get; set; } 
    public string Massage { get; set; }
    public string Type { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public DateTime Date { get; set; }
    public bool IsRead { get; set; }

}
