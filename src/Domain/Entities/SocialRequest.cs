using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class SocialRequest
{
    public int Id { get; set; }
    public bool IsResidentInFamilyHome { get; set; }
    [Required]
    public string FatherName { get; set; }
    public bool FatherIsDead { get; set; } 
    public string? FatherJob { get; set; }
    public string? FatherState { get; set; }
    public string? FatherIncome { get; set; }
    public int NumberOfFamilyMembers { get; set; }
    public int NumberOfFamilyMembersInEdu { get; set; }
    public string Status { get; set; }
    [Required]
    [ForeignKey("Service")]
    public int ServiceId { get; set; }
    public Service Service { get; set; }
    [Required]
    public string StudentId { get; set; }
    public string? ModeratorId { get; set; }
    public User Student { get; set; }
    [ForeignKey("Donation")]
    public int? DonationId { get; set; }
    [JsonIgnore]
    public virtual Donation? Donation { get; set; }
    
}
