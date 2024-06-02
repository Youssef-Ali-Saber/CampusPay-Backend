namespace Domain.Entities;

public class ConnectedUser
{
    public int Id { get; set; }
    public string userId { get; set; }
    public string ConnectionId { get; set; }
    public User user { get; set; }
}