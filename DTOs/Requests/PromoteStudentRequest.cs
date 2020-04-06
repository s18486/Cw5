using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.DTOs.Requests
{
    public class PromoteStudentRequest
    {
        [Required(ErrorMessage ="Semester field has to be specified")]
        public int Semester { get; set; }
        [Required(ErrorMessage = "Studies field has to be specified")]
        [MaxLength(100)]
        public String Studies { get; set; }
    }
}
