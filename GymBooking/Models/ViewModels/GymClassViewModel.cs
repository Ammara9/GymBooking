namespace GymBooking.Models.ViewModels
{
    public class GymClassViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndTime => StartTime + Duration; // EndTime beräknas från StartTime och Duration
        public string Description { get; set; }
        public bool IsBooked { get; set; }
    }
}
