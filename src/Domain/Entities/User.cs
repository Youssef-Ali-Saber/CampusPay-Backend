using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Domain.Entities;

public class User: IdentityUser
{
    [Required]
    public string FullName { get; set; }
    [Required]
    [MaxLength(14)]
    [MinLength(14)]
    public string SSN { get; set; }
    [DataType(DataType.Currency)]
    public decimal Balance { get; set; }
    public int Squad_Year { get; set; } = 4;
    public string College_Name { get; set; } = "FCI";
    [MaxLength(6)]
    [MinLength(6)]
    public int? VerificationCode { get; set; }
    public double? Latitude { get; set; } = 30.043489;
    public double? Longitude { get; set; } = 31.235291;
    [NotMapped]
    public IFormFile? Picture { get; set; }
    public string? FilePath { get; set; }
    public bool IsStoped { get; set; }
    [JsonIgnore]
    public List<Feedback> Feedbacks { get; set; }
    public DateOnly? DateOfBirth { get; set; } = new DateOnly(2002, 4, 17);
    public int? ZIPCode { get; set; } = 65311;
    public string? City { get; set; } = "cairo";
    public string? State { get; set; } = "CA";
    public char? Gender { get; set; } = 'M';
}
