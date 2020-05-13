using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cw5.Entity;
using Cw5.Models;
using Cw5.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cw5.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : Controller
    {

        readonly IStudentEntityService service;

        [HttpGet]
        public IActionResult GetStudent()
        {
            return Ok(service.getStudents());
        }

        [HttpPost]
        public IActionResult ModifyStudent([FromBody]Student student)
        {
            return Ok(service.modifyStudent(student));
        }

        [HttpDelete]
        public IActionResult DeleteStudent([FromQuery] string index)
        {
            return Ok(service.removeStudent(index));
        }
    }
}