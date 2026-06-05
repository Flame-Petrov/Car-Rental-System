using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CarRentalSystem_WPF.Services;
using CarRentalSystem_WPF.ViewModels;

namespace CarRentalSystem_WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize ViewModel with DataService
            var dataService = new DataService();
            _viewModel = new MainWindowViewModel(dataService);
            DataContext = _viewModel;
            
            // Subscribe to property changes to force column regeneration
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ItemsDataGrid == null) return;

            // Force DataGrid to regenerate columns when view mode changes
            if (e.PropertyName == nameof(MainWindowViewModel.CurrentViewMode))
            {
                // Clear columns immediately when view mode changes
                ItemsDataGrid.Columns.Clear();
                
                // Force regeneration after a short delay to ensure items are loaded
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (ItemsDataGrid != null && ItemsDataGrid.ItemsSource != null)
                    {
                        // Trigger column regeneration by refreshing
                        var items = ItemsDataGrid.ItemsSource;
                        ItemsDataGrid.ItemsSource = null;
                        ItemsDataGrid.ItemsSource = items;
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            // Also handle when DisplayedItems collection changes (when first item is added after clearing)
            else if (e.PropertyName == nameof(MainWindowViewModel.DisplayedItems))
            {
                // Only regenerate if columns are empty (view was just switched)
                if (ItemsDataGrid.Columns.Count == 0 && ItemsDataGrid.ItemsSource != null && 
                    ((System.Collections.ICollection)ItemsDataGrid.ItemsSource).Count > 0)
                {
                    // Columns will be auto-generated when items are set
                    var items = ItemsDataGrid.ItemsSource;
                    ItemsDataGrid.ItemsSource = null;
                    ItemsDataGrid.ItemsSource = items;
                }
            }
        }

        private void ItemsDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel == null) return;

            if (viewModel.CurrentViewMode == MainWindowViewModel.ViewMode.Cars)
            {
                // Hide DisplayName column when viewing Cars
                if (e.PropertyName == "DisplayName")
                {
                    e.Cancel = true;
                }
            }
            else if (viewModel.CurrentViewMode == MainWindowViewModel.ViewMode.Renters)
            {
                // Hide Paperworks collection when viewing Renters
                if (e.PropertyName == "Paperworks")
                {
                    e.Cancel = true;
                }
            }
            else if (viewModel.CurrentViewMode == MainWindowViewModel.ViewMode.Documents)
            {
                // Customize columns for Documents view
                switch (e.PropertyName)
                {
                    case "Id":
                        e.Column.Header = "Id";
                        break;
                    case "CarBrand":
                        e.Column.Header = "Car Brand";
                        break;
                    case "CarModel":
                        e.Column.Header = "Car Model";
                        break;
                    case "CarLicensePlate":
                        e.Column.Header = "Car License Plate";
                        break;
                    case "CarPricePerDay":
                        e.Column.Header = "Car Price Per Day";
                        if (e.Column is DataGridBoundColumn boundColumn)
                        {
                            boundColumn.Binding = new Binding("CarPricePerDay") 
                            { 
                                StringFormat = "C2" 
                            };
                        }
                        break;
                    case "RenterName":
                        e.Column.Header = "Renter Name";
                        break;
                    case "RenterPhoneNumber":
                        e.Column.Header = "Renter Phone Number";
                        break;
                    case "RentalStartDate":
                        e.Column.Header = "Start Date";
                        if (e.Column is DataGridBoundColumn boundColumnStart)
                        {
                            boundColumnStart.Binding = new Binding("RentalStartDate") 
                            { 
                                StringFormat = "MM/dd/yyyy" 
                            };
                        }
                        break;
                    case "RentalEndDate":
                        e.Column.Header = "End Date";
                        if (e.Column is DataGridBoundColumn boundColumnEnd)
                        {
                            boundColumnEnd.Binding = new Binding("RentalEndDate") 
                            { 
                                StringFormat = "MM/dd/yyyy" 
                            };
                        }
                        break;
                    case "TotalCost":
                        e.Column.Header = "Total Cost";
                        if (e.Column is DataGridBoundColumn boundColumnTotal)
                        {
                            boundColumnTotal.Binding = new Binding("TotalCost") 
                            { 
                                StringFormat = "C2" 
                            };
                        }
                        break;
                    default:
                        // Hide all other columns (CarId, RenterId, Car, Renter, CarInfo)
                        e.Cancel = true;
                        break;
                }
            }
        }
    }
}
