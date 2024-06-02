using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    [Required]
    public DateTime Date { get; set; }
    public string Status { get; set; }
    [Required]
    public string UserId { get; set; }
    [Required]
    public int ServiceId { get; set; }
    public Service Service { get; set; }
    public User User { get; set; }

}
