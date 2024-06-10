using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UserBaseDto
{

    [Required]
    [EmailAddress]
    [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")]
    public virtual string Email { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    [MaxLength(14)]
    [MinLength(14)]
    public string SSN { get; set; }
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character.")]
    public string Password { get; set; }
    [Required]
    [Compare("Password",
        ErrorMessage = "The confirmation password does not match the original password.")]
    public string ConfirmPassword { get; set; }
    public IFormFile? Picture { get; set; }
}
