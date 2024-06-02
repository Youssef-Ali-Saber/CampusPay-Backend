using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class StudentDto : UserBaseDto
{

    [Required]
    [EmailAddress]
    [RegularExpression(@"^[a-z]{2}\d{4}@fayoum.edu.eg$")]
    public override string Email { get; set; }
}
