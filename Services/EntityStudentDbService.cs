using Cw5.Entity;
using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class EntityStudentDbService : IStudentEntityService
    {
        masterContext db = new masterContext();

        public List<EStudent> getStudents()
        {
            return db.Student.ToList();
        }

        public bool modifyStudent(Student student)
        {
            var TmpStudent = db.Student.Where(s=>s.IndexNumber==student.IndexNumber).First();
            if (TmpStudent == null)
                return false;

            TmpStudent.BirthDate = student.BirthDate;
            TmpStudent.FirstName = student.FirstName;
            TmpStudent.LastName = student.LastName;

            db.SaveChanges();
            return (true);
        }

        public bool removeStudent(string index)
        {
            var TmpStudent = db.Student.Where(s => s.IndexNumber == index).First();
            if (TmpStudent == null)
                return false;

            db.Remove(TmpStudent);

            db.SaveChanges();
            return true;
        }
    }
}
