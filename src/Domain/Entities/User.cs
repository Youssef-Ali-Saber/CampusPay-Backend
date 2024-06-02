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
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    [NotMapped]
    public IFormFile? Picture { get; set; }
    public string? FilePath { get; set; }
    public bool IsStoped { get; set; }
    [JsonIgnore]
    public List<Feedback> Feedbacks { get; set; }
    [JsonIgnore]
    public List<Service?> services { get; set; }
}
