using System;
using System.Collections.Generic;

namespace Cw5.Entity
{
    public partial class EStudent
    {
        public string IndexNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public int IdEnrollment { get; set; }
        public string Pass { get; set; }
        public string Salt { get; set; }
        public string Token { get; set; }
        public string Refresh { get; set; }

        public virtual EEnrollment IdEnrollmentNavigation { get; set; }
    }
}
