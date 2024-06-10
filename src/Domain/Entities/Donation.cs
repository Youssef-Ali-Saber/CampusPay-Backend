


namespace Domain.Entities;

public class Donation
{

    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public int SocialRequestId { get; set; }
    public virtual SocialRequest SocialRequest { get; set; }
}
