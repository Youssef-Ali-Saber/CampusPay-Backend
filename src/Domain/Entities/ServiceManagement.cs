namespace Domain.Entities;

public class ServiceManagement
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int ServiceId { get; set; }
    public string UserId { get; set; }
    public Service Service { get; set; }
    public User User { get; set; }
}
