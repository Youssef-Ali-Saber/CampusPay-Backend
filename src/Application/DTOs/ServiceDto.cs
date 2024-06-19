using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ServiceDto
{
    [Required]
    public string Name { get; set; }
    public string? Description { get; set; } = null;
    [Required]
    public string Type { get; set; }
    [Required]
    public int SquadYear { get; set; }=4;
    [Required]
    public string CollegeName { get; set; }="FCI";
    [Required]
    public decimal Cost { get; set; }
    public IFormFile? Icon { get; set; }
}
