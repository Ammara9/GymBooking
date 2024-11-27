using Microsoft.AspNetCore.Identity;

namespace GymBooking.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public DateTime TimeOfRegistration { get; set; }
        public ICollection<ApplicationUserGymClass> AttendingClasses { get; set; }
    }
}
