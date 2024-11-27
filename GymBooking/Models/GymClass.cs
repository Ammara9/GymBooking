namespace GymBooking.Models
{
    public class GymClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndTime
        {
            get { return StartTime + Duration; }
        } // EndTime beräknas från StartTime och Duration
        public string Description { get; set; }

        // Relation till ApplicationUser via kopplingstabellen
        public ICollection<ApplicationUserGymClass> AttendingMembers { get; set; }

        public GymClass()
        {
            AttendingMembers = new List<ApplicationUserGymClass>(); // Initialisera kollektionen för att undvika null-referensfel.
        }
    }
}
