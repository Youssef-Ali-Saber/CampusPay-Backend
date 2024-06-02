using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Service
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; }
    [Required]
    public string Type { get; set; }
    [Required]
    [DataType(DataType.Currency)]
    public decimal Cost { get; set; }
    [Required]
    public int SquadYear { get; set; }=4;
    [Required]
    public string CollegeName { get; set; }="FCI";
    [JsonIgnore]
    public List<User> Users { get; set; }
    public string? FilePath { get; set; }

}
