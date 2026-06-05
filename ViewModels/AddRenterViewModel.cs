using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CarRentalSystem_WPF.Models;
using CarRentalSystem_WPF.Services;
using CarRentalSystem_WPF.Views;

namespace CarRentalSystem_WPF.ViewModels
{
    public class AddRenterViewModel : ViewModelBase
    {
        // Local part, "@", domain, ".", and a TLD of at least two letters.
        private static readonly Regex EmailPattern =
            new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);

        private readonly IDataService _dataService;
        private readonly Renter? _renterToEdit;

        public AddRenterViewModel(IDataService dataService, Renter? renterToEdit = null)
        {
            _dataService = dataService;
            _renterToEdit = renterToEdit;

            if (renterToEdit != null)
            {
                FullName = renterToEdit.FullName;
                DriverLicense = renterToEdit.DriverLicenseNumber;
                PhoneNumber = renterToEdit.PhoneNumber;
                Email = renterToEdit.Email;
            }

            SaveCommand = new RelayCommand(_ => SaveRenter());
            CancelCommand = new RelayCommand(_ => Cancel());
            RemoveCommand = new RelayCommand(_ => RemoveRenter(), _ => _renterToEdit != null);
        }

        public string FullName { get; set; } = string.Empty;
        public string DriverLicense { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string WindowTitle => _renterToEdit == null ? "Add New Renter" : "Edit Renter";
        public string ButtonText => _renterToEdit == null ? "Add" : "Save";
        public Visibility ShowRemoveButton => _renterToEdit != null ? Visibility.Visible : Visibility.Collapsed;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RemoveCommand { get; }

        private async void SaveRenter()
        {
            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(DriverLicense) ||
                string.IsNullOrWhiteSpace(PhoneNumber) ||
                string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!EmailPattern.IsMatch(Email.Trim()))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_renterToEdit == null)
                {
                    var newRenter = new Renter
                    {
                        FullName = FullName.Trim(),
                        DriverLicenseNumber = DriverLicense.Trim(),
                        PhoneNumber = PhoneNumber.Trim(),
                        Email = Email.Trim()
                    };
                    await _dataService.AddRenterAsync(newRenter);
                }
                else
                {
                    _renterToEdit.FullName = FullName.Trim();
                    _renterToEdit.DriverLicenseNumber = DriverLicense.Trim();
                    _renterToEdit.PhoneNumber = PhoneNumber.Trim();
                    _renterToEdit.Email = Email.Trim();
                    await _dataService.UpdateRenterAsync(_renterToEdit);
                }

                CloseDialog(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving renter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseDialog(false);

        private async void RemoveRenter()
        {
            if (_renterToEdit == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the renter '{_renterToEdit.FullName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _dataService.DeleteRenterAsync(_renterToEdit.Id);
                CloseDialog(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting renter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CloseDialog(bool result)
        {
            if (Application.Current.Windows.OfType<AddRenterWindow>().FirstOrDefault() is AddRenterWindow window)
            {
                window.DialogResult = result;
                window.Close();
            }
        }
    }
}
