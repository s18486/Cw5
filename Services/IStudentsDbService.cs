using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IStudentsDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest req);
        PromoteStudentResponse PromoteStudent(PromoteStudentRequest req);

        bool Check(string Index);

        bool Login(string Index, string Password);
        void SetRefreshToken(string index, string refresh);
        string GetRefreshTokenOwner(string refreshToken);
        PasswordDetails GetStudentPasswordData(string index);
    }
}
