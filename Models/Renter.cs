using System.Collections.ObjectModel;

namespace CarRentalSystem_WPF.Models
{
    public class Renter
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ObservableCollection<Paperwork>? Paperworks { get; set; }
    }
}
