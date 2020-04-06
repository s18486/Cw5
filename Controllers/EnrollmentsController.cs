using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        IStudentsDbService dbService;

        public EnrollmentsController(IStudentsDbService dbService)
        {
            this.dbService = dbService;
        }

        [HttpPost("{promotions}")]
        public IActionResult PromoteStudents(PromoteStudentRequest request)
        {
            PromoteStudentResponse response = dbService.PromoteStudent(request);
            if (response == null)
                return NotFound("Wrong data was passed");
            else
                return StatusCode(201,response);
        }
        [HttpPost]

        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse response=dbService.EnrollStudent(request);
            if (response == null)
                return NotFound("Wrong data was passed");
            else
                return StatusCode(201,response);
        }
    }
}