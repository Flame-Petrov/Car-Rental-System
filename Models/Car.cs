namespace CarRentalSystem_WPF.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public float RentalPricePerDay { get; set; }
        public bool IsAvailable { get; set; }

        public string DisplayName => $"{Brand} | {Model} | ({Year})  | {LicensePlate}";
    }
}
