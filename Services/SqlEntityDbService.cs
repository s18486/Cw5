using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class SqlEntityDbService : IStudentsDbService
    {
        masterContext db = new masterContext();
        readonly IPasswordService passwordService;

        public SqlEntityDbService(IPasswordService password)
        {
            this.passwordService = password;
        }
        public PromoteStudentResponse PromoteStudent(PromoteStudentRequest request)
        {
            var enroll = db.Studies.Join(db.Enrollment,
                s => s.IdStudy,
                en => en.IdStudy,
                (s, e) => new { e, s.Name, s.IdStudy })
                .Where(t => t.Name == request.Studies && t.e.Semester == request.Semester).First();
            if (enroll!=null)
            {
                var tmp = db.Enrollment.Where(e => e.IdStudy == enroll.IdStudy && e.Semester == enroll.e.Semester);
                foreach(var t in tmp)
                    t.Semester = t.Semester + 1;
                db.SaveChanges();
                PromoteStudentResponse response = new PromoteStudentResponse
                {
                    IdEnrollment = enroll.e.IdEnrollment,
                    Semester = enroll.e.Semester,
                    Study = enroll.Name,
                    StartDate = enroll.e.StartDate
                };
                return response;
            }
            else return null;
        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            if (db.Student.Any(s => s.IndexNumber == request.IndexNumber))
                return null;

            var study = db.Studies.Where(s => s.Name == request.Studies).First();
            if (study   == null)
                return null;

            var enr = db.Enrollment.Where(en => en.StartDate
                .Equals(db.Enrollment.Where(e => e.Semester == 1 && e.IdStudy == study.IdStudy)
                    .Max(e => e.StartDate))).First(); ;

            if (enr != null){
                String salt = passwordService.CreateSalt();
                db.Student.Add(new EStudent
                {
                    IndexNumber = request.IndexNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.Birthdate,
                    IdEnrollment = enr.IdEnrollment,
                    Pass = passwordService.HashPassword(request.Password, salt),
                    Salt = salt                  
                });
                db.SaveChanges();
                return new EnrollStudentResponse
                {
                    IdEnrollment = enr.IdEnrollment,
                    Semester = enr.Semester,
                    Study = study.Name,
                    StartDate = enr.StartDate
                };
            }
            else
            {
                db.Enrollment.Add(new EEnrollment
                {
                    IdEnrollment = db.Enrollment.Max(e=>e.IdEnrollment),
                    Semester = 1,
                    IdStudy = db.Studies.Max(s=>s.IdStudy),
                    StartDate = DateTime.Today
                });
                String salt = passwordService.CreateSalt();
                db.Student.Add(new EStudent
                {
                    IndexNumber = request.IndexNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.Birthdate,
                    IdEnrollment = enr.IdEnrollment,
                    Pass = passwordService.HashPassword(request.Password, salt),
                    Salt = salt
                });
                db.SaveChanges();
                return new EnrollStudentResponse
                {
                    IdEnrollment = enr.IdEnrollment,
                    Semester = enr.Semester,
                    Study = study.Name,
                    StartDate = enr.StartDate
                };
            }

                   
        }
        public bool Check(string index)
        {
            return db.Student.Any(s => s.IndexNumber == index);
        }

        public bool Login(string Index, string Password)
        {
            return db.Student.Any(s => s.IndexNumber == Index && s.Pass == Password);
        }

        public void SetRefreshToken(string IndexNumber, string refresh)
        {
            var tmp = db.Student.Where(s => s.IndexNumber == IndexNumber).First();
            tmp.Refresh = refresh;
            db.SaveChanges();
        }

        public string GetRefreshTokenOwner(string refreshToken)
        {
                var tmp = db.Student.Where(s => s.Refresh == refreshToken).First();
               
                return tmp == null ? null : tmp.IndexNumber;
           
        }

        public PasswordDetails GetStudentPasswordData(string IndexNumber)
        {
            PasswordDetails response = new PasswordDetails();
             try
             {
                 var tmp = db.Student.Where(s=>s.IndexNumber == IndexNumber).Select(s=>new { Password = s.Pass, Salt = s.Salt}).First();             

                 if (tmp != null)
                 {
                     response.Password = tmp.Password;
                     response.Salt = tmp.Salt;
                     return response;
                 }
                 return null;
             }
             catch (Exception)
             {
                 return null;
             }
        }
    }
}
