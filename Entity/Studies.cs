using System;
using System.Collections.Generic;

namespace Cw5.Entity
{
    public partial class Studies
    {
        public Studies()
        {
            Enrollment = new HashSet<EEnrollment>();
        }

        public int IdStudy { get; set; }
        public string Name { get; set; }

        public virtual ICollection<EEnrollment> Enrollment { get; set; }
    }
}
