using System;
using System.Collections.Generic;

namespace Cw5.Entity
{
    public partial class EEnrollment
    {
        public EEnrollment()
        {
            Student = new HashSet<EStudent>();
        }

        public int IdEnrollment { get; set; }
        public int Semester { get; set; }
        public int IdStudy { get; set; }
        public DateTime StartDate { get; set; }

        public virtual Studies IdStudyNavigation { get; set; }
        public virtual ICollection<EStudent> Student { get; set; }
    }
}
