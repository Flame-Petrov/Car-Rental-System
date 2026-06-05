using System.Windows;
using System.Windows.Input;
using CarRentalSystem_WPF.Models;
using CarRentalSystem_WPF.Services;
using CarRentalSystem_WPF.Views;

namespace CarRentalSystem_WPF.ViewModels
{
    public class AddCarViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Car? _carToEdit;

        public AddCarViewModel(IDataService dataService, Car? carToEdit = null)
        {
            _dataService = dataService;
            _carToEdit = carToEdit;

            if (carToEdit != null)
            {
                Brand = carToEdit.Brand;
                Model = carToEdit.Model;
                Year = carToEdit.Year.ToString();
                LicensePlate = carToEdit.LicensePlate;
                Price = carToEdit.RentalPricePerDay.ToString();
            }

            SaveCommand = new RelayCommand(_ => SaveCar());
            CancelCommand = new RelayCommand(_ => Cancel());
            RemoveCommand = new RelayCommand(_ => RemoveCar(), _ => _carToEdit != null);
        }

        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;

        public string WindowTitle => _carToEdit == null ? "Add New Car" : "Edit Car";
        public string ButtonText => _carToEdit == null ? "Add" : "Save";
        public Visibility ShowRemoveButton => _carToEdit != null ? Visibility.Visible : Visibility.Collapsed;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RemoveCommand { get; }

        private async void SaveCar()
        {
            if (string.IsNullOrWhiteSpace(Brand) ||
                string.IsNullOrWhiteSpace(Model) ||
                string.IsNullOrWhiteSpace(LicensePlate) ||
                string.IsNullOrWhiteSpace(Year) ||
                string.IsNullOrWhiteSpace(Price))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(Year.Trim(), out var year))
            {
                MessageBox.Show("Year must be a whole number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!float.TryParse(Price.Trim(), out var price) || price < 0)
            {
                MessageBox.Show("Rental price must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_carToEdit == null)
                {
                    var newCar = new Car
                    {
                        Brand = Brand.Trim(),
                        Model = Model.Trim(),
                        Year = year,
                        LicensePlate = LicensePlate.Trim(),
                        RentalPricePerDay = price,
                        IsAvailable = true
                    };
                    await _dataService.AddCarAsync(newCar);
                }
                else
                {
                    _carToEdit.Brand = Brand.Trim();
                    _carToEdit.Model = Model.Trim();
                    _carToEdit.Year = year;
                    _carToEdit.LicensePlate = LicensePlate.Trim();
                    _carToEdit.RentalPricePerDay = price;
                    await _dataService.UpdateCarAsync(_carToEdit);
                }

                CloseDialog(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving car: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseDialog(false);

        private async void RemoveCar()
        {
            if (_carToEdit == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the car '{_carToEdit.Brand} {_carToEdit.Model}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _dataService.DeleteCarAsync(_carToEdit.Id);
                CloseDialog(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting car: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CloseDialog(bool result)
        {
            if (Application.Current.Windows.OfType<AddCarWindow>().FirstOrDefault() is AddCarWindow window)
            {
                window.DialogResult = result;
                window.Close();
            }
        }
    }
}
