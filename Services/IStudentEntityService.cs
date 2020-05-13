using Cw5.Entity;
using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public interface IStudentEntityService
    {
        public List<EStudent> getStudents();
        public bool removeStudent(string index);
        public bool modifyStudent(Student student);

    }
}
