using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentalSystem_WPF.Models;
using CarRentalSystem_WPF.Services;
using CarRentalSystem_WPF.Views;

namespace CarRentalSystem_WPF.ViewModels
{
    public class AddDocumentViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Paperwork? _documentToEdit;
        private Car? _selectedCar;
        private Renter? _selectedRenter;
        private DateTime _startDate = DateTime.Today;
        private DateTime _endDate = DateTime.Today.AddDays(1);
        private string _totalCost = string.Empty;

        public AddDocumentViewModel(IDataService dataService, Paperwork? documentToEdit = null)
        {
            _dataService = dataService;
            _documentToEdit = documentToEdit;

            SaveCommand = new RelayCommand(_ => SaveDocument());
            CancelCommand = new RelayCommand(_ => Cancel());
            RemoveCommand = new RelayCommand(_ => RemoveDocument(), _ => _documentToEdit != null);

            _ = LoadDataAsync();
        }

        public ObservableCollection<Car> Cars { get; } = new();
        public ObservableCollection<Renter> Renters { get; } = new();

        public Car? SelectedCar
        {
            get => _selectedCar;
            set
            {
                if (SetProperty(ref _selectedCar, value))
                {
                    CalculateTotalCost();
                }
            }
        }

        public Renter? SelectedRenter
        {
            get => _selectedRenter;
            set => SetProperty(ref _selectedRenter, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    CalculateTotalCost();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    CalculateTotalCost();
                }
            }
        }

        public string TotalCost
        {
            get => _totalCost;
            set => SetProperty(ref _totalCost, value);
        }

        public string WindowTitle => _documentToEdit == null ? "Add New Document" : "Edit Document";
        public string ButtonText => _documentToEdit == null ? "Add" : "Save";
        public Visibility ShowRemoveButton => _documentToEdit != null ? Visibility.Visible : Visibility.Collapsed;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RemoveCommand { get; }

        private async Task LoadDataAsync()
        {
            try
            {
                var cars = await _dataService.GetAllCarsAsync();
                var renters = await _dataService.GetAllRentersAsync();
                var documents = await _dataService.GetAllDocumentsAsync();

                Cars.Clear();
                Renters.Clear();

                // Cars with an active (not yet expired) contract are unavailable,
                // except the one assigned to the document currently being edited.
                var today = DateTime.Today;
                var busyCarIds = documents
                    .Where(d => d.RentalEndDate > today &&
                                (_documentToEdit == null || d.Id != _documentToEdit.Id))
                    .Select(d => d.CarId)
                    .ToHashSet();

                foreach (var car in cars.Where(c => !busyCarIds.Contains(c.Id)))
                {
                    Cars.Add(car);
                }

                foreach (var renter in renters)
                {
                    Renters.Add(renter);
                }

                if (_documentToEdit != null)
                {
                    SelectedCar = Cars.FirstOrDefault(c => c.Id == _documentToEdit.CarId);
                    SelectedRenter = Renters.FirstOrDefault(r => r.Id == _documentToEdit.RenterId);
                    StartDate = _documentToEdit.RentalStartDate;
                    EndDate = _documentToEdit.RentalEndDate;
                }

                CalculateTotalCost();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveDocument()
        {
            var car = SelectedCar;
            var renter = SelectedRenter;

            if (car == null)
            {
                MessageBox.Show("Please select a car.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (renter == null)
            {
                MessageBox.Show("Please select a renter.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EndDate <= StartDate)
            {
                MessageBox.Show("End date must be after start date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var totalCost = CalculateTotalCostValue();
            if (totalCost <= 0)
            {
                MessageBox.Show("Total cost must be greater than zero. Please check the car selection and dates.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Re-check at save time that the car has no conflicting active contract
                // (guards against the form being open while another booking is made).
                var allDocs = await _dataService.GetAllDocumentsAsync();
                var today = DateTime.Today;
                bool carIsBooked = allDocs.Any(d =>
                    d.CarId == car.Id &&
                    d.RentalEndDate > today &&
                    (_documentToEdit == null || d.Id != _documentToEdit.Id));

                if (carIsBooked)
                {
                    MessageBox.Show(
                        $"'{car.Brand} {car.Model}' already has an active rental contract.\nPlease select a different car.",
                        "Car Unavailable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (_documentToEdit == null)
                {
                    // New contract: reserve the selected car.
                    car.IsAvailable = false;
                    await _dataService.UpdateCarAsync(car);

                    var newDocument = new Paperwork
                    {
                        CarId = car.Id,
                        Car = car,
                        RenterId = renter.Id,
                        Renter = renter,
                        RentalStartDate = StartDate,
                        RentalEndDate = EndDate,
                        TotalCost = totalCost
                    };
                    await _dataService.AddDocumentAsync(newDocument);
                }
                else
                {
                    // Edit: if the car changed, free the old one and reserve the new one.
                    if (_documentToEdit.CarId != car.Id)
                    {
                        var oldCar = await _dataService.GetCarByIdAsync(_documentToEdit.CarId);
                        if (oldCar != null)
                        {
                            oldCar.IsAvailable = true;
                            await _dataService.UpdateCarAsync(oldCar);
                        }

                        car.IsAvailable = false;
                        await _dataService.UpdateCarAsync(car);
                    }

                    _documentToEdit.CarId = car.Id;
                    _documentToEdit.Car = car;
                    _documentToEdit.RenterId = renter.Id;
                    _documentToEdit.Renter = renter;
                    _documentToEdit.RentalStartDate = StartDate;
                    _documentToEdit.RentalEndDate = EndDate;
                    _documentToEdit.TotalCost = totalCost;
                    await _dataService.UpdateDocumentAsync(_documentToEdit);
                }

                CloseDialog(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotalCost()
        {
            var totalCost = CalculateTotalCostValue();
            TotalCost = totalCost > 0 ? totalCost.ToString("F2") : "0.00";
        }

        private float CalculateTotalCostValue()
        {
            if (SelectedCar == null) return 0;

            float days = (EndDate - StartDate).Days + 1; // Inclusive of both start and end date
            return SelectedCar.RentalPricePerDay * days;
        }

        private void Cancel() => CloseDialog(false);

        private async void RemoveDocument()
        {
            if (_documentToEdit == null) return;

            var carInfo = $"{_documentToEdit.Car?.Brand} {_documentToEdit.Car?.Model}".Trim();
            if (string.IsNullOrEmpty(carInfo)) carInfo = "Unknown Car";
            var renterInfo = _documentToEdit.Renter?.FullName ?? "Unknown Renter";

            var result = MessageBox.Show(
                $"Are you sure you want to delete this contract?\n\nCar: {carInfo}\nRenter: {renterInfo}\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _dataService.DeleteDocumentAsync(_documentToEdit.Id);
                CloseDialog(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting document: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CloseDialog(bool result)
        {
            if (Application.Current.Windows.OfType<AddDocumentWindow>().FirstOrDefault() is AddDocumentWindow window)
            {
                window.DialogResult = result;
                window.Close();
            }
        }
    }
}
