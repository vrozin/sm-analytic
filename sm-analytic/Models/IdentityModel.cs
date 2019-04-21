using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sm_analytic.Models
{
    public class IdentityCustomModel : IdentityUser
    {
        public IdentityCustomModel()
        {
            LastLoginDate         = DateTime.Now;
            DOB                   = DateTime.Now.AddYears(-30);
            EmailConfirmed        = false;
            EmailConfirmedByAdmin = false;
            PhoneNumberConfirmed  = true;
            LockoutEnabled        = false;
            IsApplicationAdmin    = false;
            //LockoutEnd            = DateTimeOffset.Now.AddYears(10); // To be lifted after account confirmation
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime LastLoginDate { get; set; }

        public string SecondaryPassword { get; set; }

        public DateTime DOB { get; set; }

        public bool EmailConfirmedByAdmin { get; set; }

        public bool IsApplicationAdmin { get; set; }

        //public int Id { get; set; }

        //public string Email { get; set; }

        //public string RoleName { get; set; }

        //public string Password { get; set; }

        //public bool IsValid { get; set; }

        //public virtual RequestTracker RequestTracker { get; set; }
    }
}
