using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Feedback
{
    public int Id { get; set; }
    [Required]
    public string FeedBackMassage { get; set; }
    [Required]
    public DateTime FeedBack_DateTime { get; set; }
    [Required]    
    public string UserId { get; set; }
    public User User { get; set; }

}
