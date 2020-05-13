using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw5.Controllers
{
    [Authorize(Roles = "employee")]
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        readonly IStudentsDbService dbService;
        readonly IConfiguration config;
        readonly IPasswordService passwordService;

        public EnrollmentsController(IStudentsDbService dbService, IConfiguration config, IPasswordService passwordService)
        {
            this.dbService = dbService;
            this.config = config;
            this.passwordService = passwordService;
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

        [AllowAnonymous]
        [HttpPost("refresh-token/{refreshToken}")]
        public IActionResult RefreshToken(String refreshToken)
        {
            string login = dbService.GetRefreshTokenOwner(refreshToken);
            if (login == null)
                return BadRequest("Wrong refresh token was passed");

            var claims = new[]
           {
                new Claim(ClaimTypes.Name,login),
                new Claim(ClaimTypes.Role,"employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "admin",
                audience: "employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {

            if (!dbService.Check(request.Login))
                return BadRequest("Wrong password or login");

            var requestedPasswordsData = dbService.GetStudentPasswordData(request.Login);
            if (!passwordService.ValidatePassword(requestedPasswordsData.Password, request.Password, requestedPasswordsData.Salt))
                return BadRequest("Wrong password or login");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,request.Login),
                new Claim(ClaimTypes.Role,"employee")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["SecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "admin",
                audience: "employee",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );
            var TmpRefreshToken = Guid.NewGuid();
            dbService.SetRefreshToken(request.Login, TmpRefreshToken.ToString());
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refershToken = TmpRefreshToken
            });
        }
    }
}