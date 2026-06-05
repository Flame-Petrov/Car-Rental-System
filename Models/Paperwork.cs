namespace CarRentalSystem_WPF.Models
{
    public class Paperwork
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; } = new();
        public int RenterId { get; set; }
        public Renter? Renter { get; set; } = new();
        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }
        public float TotalCost { get; set; }

        // Display-only properties for the Documents DataGrid (not stored in the database)
        public string CarBrand => Car?.Brand ?? "N/A";
        public string CarModel => Car?.Model ?? "N/A";
        public string CarLicensePlate => Car?.LicensePlate ?? "N/A";
        public float CarPricePerDay => Car?.RentalPricePerDay ?? 0;
        public string RenterName => Renter?.FullName ?? "N/A";
        public string RenterPhoneNumber => Renter?.PhoneNumber ?? "N/A";
    }
}
