using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class SocialRequestDto
{
    [Required]
    public bool IsResidentInFamilyHome { get; set; }
    [Required]
    public string FatherName { get; set; }
    [Required]
    public bool FatherIsDead { get; set; }
    public string? FatherJob { get; set; }
    public string? FatherState { get; set; }
    public string? FatherIncome { get; set; }
    [Required]
    public int NumberOfFamilyMembers { get; set; }
    [Required]
    public int NumberOfFamilyMembersInEdu { get; set; }
    [Required]
    public int ServiceId { get; set; }
}
