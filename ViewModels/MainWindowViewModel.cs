using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CarRentalSystem_WPF.Models;
using CarRentalSystem_WPF.Services;
using CarRentalSystem_WPF.Views;

namespace CarRentalSystem_WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private string _searchText = string.Empty;
        private string _selectedFilter = "All";
        private object? _selectedItem;
        private bool _isLoading;

        public MainWindowViewModel(IDataService dataService)
        {
            _dataService = dataService;

            Cars = new ObservableCollection<Car>();
            Renters = new ObservableCollection<Renter>();
            Documents = new ObservableCollection<Paperwork>();


            CarsButtonCommand = new RelayCommand(_ => SwitchToCarsView());

            RentersButtonCommand = new RelayCommand(_ => SwitchToRentersView());

            DocumentsButtonCommand = new RelayCommand(_ => SwitchToDocumentsView());

            AddCommand = new RelayCommand(_ => AddItem());

            EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);

            _ = InitializeAsync();
        }

        public ObservableCollection<Car> Cars { get; }
        public ObservableCollection<Renter> Renters { get; }
        public ObservableCollection<Paperwork> Documents { get; }

        public ObservableCollection<object> DisplayedItems { get; } = new ObservableCollection<object>();
        public ObservableCollection<string> FilterOptions { get; } = new ObservableCollection<string>();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplySearchFilter();
                }
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    ApplySearchFilter();
                }
            }
        }

        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private ViewMode _currentViewMode = ViewMode.Cars;

        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            private set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    OnPropertyChanged(nameof(IsCarsView));
                    OnPropertyChanged(nameof(IsRentersView));
                    OnPropertyChanged(nameof(IsDocumentsView));
                    OnPropertyChanged(nameof(CurrentSectionTitle));
                }
            }
        }

        public bool IsCarsView => _currentViewMode == ViewMode.Cars;
        public bool IsRentersView => _currentViewMode == ViewMode.Renters;
        public bool IsDocumentsView => _currentViewMode == ViewMode.Documents;

        public string CurrentSectionTitle => _currentViewMode switch
        {
            ViewMode.Cars => "Cars",
            ViewMode.Renters => "Renters",
            ViewMode.Documents => "Documents",
            _ => string.Empty
        };

        public ICommand CarsButtonCommand { get; }
        public ICommand RentersButtonCommand { get; }
        public ICommand DocumentsButtonCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                await _dataService.InitializeDatabaseAsync();
                // Update expired contracts on startup to ensure car availability is correct
                await _dataService.UpdateExpiredContractsAsync();
                await LoadCarsAsync();
                UpdateFilterDropdown();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void SwitchToCarsView()
        {
            CurrentViewMode = ViewMode.Cars;
            DisplayedItems.Clear();
            SelectedItem = null;
            UpdateFilterDropdown();
            // Update expired contracts when switching to cars view to ensure availability is current
            await _dataService.UpdateExpiredContractsAsync();
            await LoadCarsAsync();
        }

        private void SwitchToRentersView()
        {
            CurrentViewMode = ViewMode.Renters;
            DisplayedItems.Clear();
            SelectedItem = null;
            UpdateFilterDropdown();
            _ = LoadRentersAsync();
        }

        private void SwitchToDocumentsView()
        {
            CurrentViewMode = ViewMode.Documents;
            DisplayedItems.Clear();
            SelectedItem = null;
            UpdateFilterDropdown();
            _ = LoadDocumentsAsync();
        }

        private void UpdateFilterDropdown()
        {
            FilterOptions.Clear();
            FilterOptions.Add("All");

            switch (CurrentViewMode)
            {
                case ViewMode.Cars:
                    FilterOptions.Add("Brand");
                    FilterOptions.Add("Model");
                    FilterOptions.Add("Year");
                    FilterOptions.Add("License Plate");
                    FilterOptions.Add("Price");
                    FilterOptions.Add("Available");
                    FilterOptions.Add("Not Available");
                    break;
                case ViewMode.Renters:
                    FilterOptions.Add("Name");
                    FilterOptions.Add("Driver License Number");
                    FilterOptions.Add("Phone Number");
                    FilterOptions.Add("Email");
                    break;
                case ViewMode.Documents:
                    FilterOptions.Add("Car Brand");
                    FilterOptions.Add("Car Model");
                    FilterOptions.Add("Car License Plate");
                    FilterOptions.Add("Car Price Per Day");
                    FilterOptions.Add("Renter Name");
                    FilterOptions.Add("Renter Phone Number");
                    FilterOptions.Add("Start Date");
                    FilterOptions.Add("End Date");
                    FilterOptions.Add("Final Price");
                    break;
            }

            SelectedFilter = "All";
        }

        private async Task LoadCarsAsync()
        {
            try
            {
                // Update expired contracts before loading to ensure availability is current
                await _dataService.UpdateExpiredContractsAsync();
                var cars = await _dataService.GetAllCarsAsync();
                Cars.Clear();
                foreach (var car in cars)
                {
                    Cars.Add(car);
                }
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cars: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRentersAsync()
        {
            try
            {
                var renters = await _dataService.GetAllRentersAsync();
                Renters.Clear();
                foreach (var renter in renters)
                {
                    Renters.Add(renter);
                }
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading renters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDocumentsAsync()
        {
            try
            {
                var documents = await _dataService.GetAllDocumentsAsync();
                Documents.Clear();
                foreach (var doc in documents)
                {
                    Documents.Add(doc);
                }
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading documents: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearchFilter()
        {
            var searchTerm = SearchText?.ToLower() ?? string.Empty;
            
            // Clear first to trigger column regeneration
            DisplayedItems.Clear();

            switch (CurrentViewMode)
            {
                case ViewMode.Cars:
                    IEnumerable<Car> filteredCars = Cars;

                    // Availability filters narrow the list on their own (no search term needed).
                    if (SelectedFilter == "Available")
                        filteredCars = filteredCars.Where(car => car.IsAvailable);
                    else if (SelectedFilter == "Not Available")
                        filteredCars = filteredCars.Where(car => !car.IsAvailable);

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        filteredCars = filteredCars.Where(car => SelectedFilter switch
                        {
                            "Brand" => car.Brand?.ToLower().Contains(searchTerm) == true,
                            "Model" => car.Model?.ToLower().Contains(searchTerm) == true,
                            "Year" => car.Year.ToString().Contains(searchTerm),
                            "License Plate" => car.LicensePlate?.ToLower().Contains(searchTerm) == true,
                            "Price" => car.RentalPricePerDay.ToString().Contains(searchTerm),
                            // "All", "Available", "Not Available": match across every field
                            _ => car.Brand?.ToLower().Contains(searchTerm) == true ||
                                 car.Model?.ToLower().Contains(searchTerm) == true ||
                                 car.LicensePlate?.ToLower().Contains(searchTerm) == true ||
                                 car.Year.ToString().Contains(searchTerm) ||
                                 car.RentalPricePerDay.ToString().Contains(searchTerm) ||
                                 (car.IsAvailable ? "available" : "not available").Contains(searchTerm)
                        });
                    }

                    foreach (var car in filteredCars)
                    {
                        DisplayedItems.Add(car);
                    }
                    break;

                case ViewMode.Renters:
                    var filteredRenters = string.IsNullOrWhiteSpace(searchTerm)
                        ? Renters
                        : Renters.Where(renter =>
                        {
                            if (SelectedFilter == "All")
                            {
                                return renter.FullName?.ToLower().Contains(searchTerm) == true ||
                                       renter.Email?.ToLower().Contains(searchTerm) == true ||
                                       renter.PhoneNumber?.ToLower().Contains(searchTerm) == true ||
                                       renter.DriverLicenseNumber?.ToLower().Contains(searchTerm) == true;
                            }
                            else if (SelectedFilter == "Name")
                                return renter.FullName?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Driver License Number")
                                return renter.DriverLicenseNumber?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Phone Number")
                                return renter.PhoneNumber?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Email")
                                return renter.Email?.ToLower().Contains(searchTerm) == true;
                            return true;
                        });

                    foreach (var renter in filteredRenters)
                    {
                        DisplayedItems.Add(renter);
                    }
                    break;

                case ViewMode.Documents:
                    var filteredDocuments = string.IsNullOrWhiteSpace(searchTerm)
                        ? Documents
                        : Documents.Where(doc =>
                        {
                            if (SelectedFilter == "All")
                            {
                                return doc.Car?.Brand?.ToLower().Contains(searchTerm) == true ||
                                       doc.Car?.Model?.ToLower().Contains(searchTerm) == true ||
                                       doc.Car?.LicensePlate?.ToLower().Contains(searchTerm) == true ||
                                       doc.Car?.RentalPricePerDay.ToString().Contains(searchTerm) == true ||
                                       doc.Renter?.FullName?.ToLower().Contains(searchTerm) == true ||
                                       doc.Renter?.PhoneNumber?.ToLower().Contains(searchTerm) == true ||
                                       doc.RentalStartDate.ToString("MM/dd/yyyy").Contains(searchTerm) ||
                                       doc.RentalEndDate.ToString("MM/dd/yyyy").Contains(searchTerm) ||
                                       doc.TotalCost.ToString().Contains(searchTerm);
                            }
                            else if (SelectedFilter == "Car Brand")
                                return doc.Car?.Brand?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Car Model")
                                return doc.Car?.Model?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Car License Plate")
                                return doc.Car?.LicensePlate?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Car Price Per Day")
                                return doc.Car?.RentalPricePerDay.ToString().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Renter Name")
                                return doc.Renter?.FullName?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Renter Phone Number")
                                return doc.Renter?.PhoneNumber?.ToLower().Contains(searchTerm) == true;
                            else if (SelectedFilter == "Start Date")
                                return doc.RentalStartDate.ToString("MM/dd/yyyy").Contains(searchTerm);
                            else if (SelectedFilter == "End Date")
                                return doc.RentalEndDate.ToString("MM/dd/yyyy").Contains(searchTerm);
                            else if (SelectedFilter == "Final Price")
                                return doc.TotalCost.ToString().Contains(searchTerm);
                            return true;
                        });

                    foreach (var doc in filteredDocuments)
                    {
                        DisplayedItems.Add(doc);
                    }
                    break;
            }
        }

        private void AddItem()
        {
            switch (CurrentViewMode)
            {
                case ViewMode.Cars:
                    var addCarVm = new AddCarViewModel(_dataService);
                    var addCarWindow = new AddCarWindow { DataContext = addCarVm };
                    if (addCarWindow.ShowDialog() == true)
                    {
                        _ = LoadCarsAsync();
                    }
                    break;

                case ViewMode.Renters:
                    var addRenterVm = new AddRenterViewModel(_dataService);
                    var addRenterWindow = new AddRenterWindow { DataContext = addRenterVm };
                    if (addRenterWindow.ShowDialog() == true)
                    {
                        _ = LoadRentersAsync();
                    }
                    break;

                case ViewMode.Documents:
                    var addDocVm = new AddDocumentViewModel(_dataService);
                    var addDocWindow = new AddDocumentWindow { DataContext = addDocVm };
                    if (addDocWindow.ShowDialog() == true)
                    {
                        _ = LoadDocumentsAsync();
                        // Also reload cars to reflect updated availability after contract creation
                        _ = LoadCarsAsync();
                    }
                    break;
            }
        }

        private void EditItem()
        {
            if (SelectedItem == null) return;

            switch (CurrentViewMode)
            {
                case ViewMode.Cars:
                    if (SelectedItem is Car car)
                    {
                        var editCarVm = new AddCarViewModel(_dataService, car);
                        var editCarWindow = new AddCarWindow { DataContext = editCarVm };
                        if (editCarWindow.ShowDialog() == true)
                        {
                            _ = LoadCarsAsync();
                        }
                    }
                    break;

                case ViewMode.Renters:
                    if (SelectedItem is Renter renter)
                    {
                        var editRenterVm = new AddRenterViewModel(_dataService, renter);
                        var editRenterWindow = new AddRenterWindow { DataContext = editRenterVm };
                        if (editRenterWindow.ShowDialog() == true)
                        {
                            _ = LoadRentersAsync();
                        }
                    }
                    break;

                case ViewMode.Documents:
                    if (SelectedItem is Paperwork document)
                    {
                        var editDocVm = new AddDocumentViewModel(_dataService, document);
                        var editDocWindow = new AddDocumentWindow { DataContext = editDocVm };
                        if (editDocWindow.ShowDialog() == true)
                        {
                            _ = LoadDocumentsAsync();
                            // Also reload cars to reflect updated availability after contract edit
                            _ = LoadCarsAsync();
                        }
                    }
                    break;
            }
        }

        public enum ViewMode
        {
            Cars,
            Renters,
            Documents
        }
    }
}

