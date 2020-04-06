using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        [Required(ErrorMessage = "Student number field was not specified")]
        [RegularExpression("^s[0-9]+$")]
        public string IndexNumber { get; set; }

        [Required(ErrorMessage = "Name field was not specified")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name field was not specified")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Birth Date field was not specified or it was specified incorrect")]
        public DateTime Birthdate { get; set; }

        [Required(ErrorMessage = "Studies field was not specified")]
        [MaxLength(100)]
        public string Studies { get; set; }
    }
}
