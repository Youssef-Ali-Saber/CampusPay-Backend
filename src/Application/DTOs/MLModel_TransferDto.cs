using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class MLModel_TransferDto
    {
        [RegularExpression(@"^\d{14}$", ErrorMessage = "SSN must be exactly 14 digits.")]
        public string SSN { get; set; }

        public float Balance { get; set; }

        public float Longitude { get; set; }

        public float Latitude { get; set; }
    }
}
