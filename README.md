<img width="1036" height="685" alt="Screenshot 2026-06-05 110920" src="https://github.com/user-attachments/assets/cacca4f2-4c29-4e1e-9e4d-7e429108bc3f" />
<img width="1037" height="684" alt="Screenshot 2026-06-05 110815" src="https://github.com/user-attachments/assets/10d47f9a-a2f2-401a-9fe1-8fc2d18dfa0e" />
# Car Rental System

A Windows desktop application for managing a small car rental business. It keeps
track of the rental fleet, the customers who rent the cars, and the rental
contracts that connect them. Built with **WPF on .NET 8** using the **MVVM**
pattern and **Entity Framework Core** over a local **SQLite** database.

## What the system is designed to do

The application manages three things and the relationships between them:

- **Cars** - the rental fleet. Each car has a brand, model, year, license plate, a
  daily rental price, and an availability status.
- **Renters** - the customers. Each renter has a name, driver's license number,
  phone number, and email.
- **Documents (rental contracts)** - each contract assigns one car to one renter for
  a chosen date range. The total cost is calculated automatically from the car's
  daily price and the number of days.

Around this core data, the system provides:

- Full create / edit / delete for cars, renters, and contracts, with input
  validation on every form.
- Per-section search with field-specific filters (e.g. find cars by brand, year, or
  availability).
- **Automatic availability management** - a car is marked unavailable while it has an
  active contract, can't be double-booked, and is automatically freed again once its
  contract's end date passes (or when the contract is deleted).

## How it can be expanded

The MVVM structure and the `IDataService` abstraction make the system
straightforward to extend. Some natural next steps:

- **New fields** - add a property to a model in `Models/`, surface it in the matching
  view model and view, and EF Core will include it in the database.
- **New entities** - add a model, register it as a `DbSet` in
  `Data/DatabaseContext.cs`, add the corresponding methods to `IDataService` /
  `DataService`, then build a view model and view following the existing
  add/edit pattern.
- **Swap or extend the data layer** - because all data access goes through the
  `IDataService` interface, you can replace SQLite with another database provider,
  or add a mock implementation for testing, without touching the view models.
- **Reporting & analytics** - add new read methods to the data service (e.g. revenue
  per period, most-rented cars) and a view to display them.
- **Authentication / roles, payment tracking, PDF contract export, or EF Core
  migrations** for versioned schema changes are all additive on top of the current
  layers.

## Project structure

The application lives in the `CarRentalSystem_WPF/` project, organised by
responsibility:

### `Models/` - the data entities

Plain C# classes that define the shape of the data (and the database tables).

- **`Car.cs`** - a vehicle: brand, model, year, license plate, daily price, and
  availability flag.
- **`Renter.cs`** - a customer: full name, driver's license number, phone, and email.
- **`Paperwork.cs`** - a rental contract linking a car and a renter over a date
  range, with the total cost. Also exposes a few display-only properties used by the
  UI grid.

### `Data/` - database access

- **`DatabaseContext.cs`** - the Entity Framework Core `DbContext`. Configures the
  SQLite connection, declares the Cars / Renters / Paperworks tables, and defines
  keys, required fields, and the relationships between contracts, cars, and renters.

### `Services/` - business logic and data operations

The single layer that talks to the database, hiding EF Core from the rest of the app.

- **`IDataService.cs`** - the interface defining all available operations (CRUD for
  cars, renters, and documents, plus database setup and expired-contract handling).
- **`DataService.cs`** - the implementation. Performs the actual database reads and
  writes and contains the availability rules (e.g. freeing a car when its contract
  expires or is deleted).

### `ViewModels/` - presentation logic

The "VM" of MVVM: each view model exposes data and commands that the views bind to,
keeping logic out of the UI markup.

- **`MainWindowViewModel.cs`** - drives the main window: switches between the Cars,
  Renters, and Documents sections, loads data, and runs the search/filter logic.
- **`AddCarViewModel.cs`** - add/edit/delete logic and validation for a car.
- **`AddRenterViewModel.cs`** - add/edit/delete logic and validation for a renter
  (including email-format validation).
- **`AddDocumentViewModel.cs`** - add/edit/delete logic for a rental contract:
  offers only available cars, calculates the cost live, and enforces the
  no-double-booking rule.
- **`ViewModelBase.cs`** - shared base class implementing `INotifyPropertyChanged`
  so the UI updates when data changes.
- **`RelayCommand.cs`** - a reusable `ICommand` that lets view models expose button
  actions to the views.

### `Views/` - the user interface (WPF / XAML)

The windows the user sees, bound to the view models above.

- **`MainWindow.xaml`** - the main screen with section navigation, the search/filter
  bar, and the data grid.
- **`AddCarWindow.xaml`** - dialog for adding or editing a car.
- **`AddRenterWindow.xaml`** - dialog for adding or editing a renter.
- **`AddDocumentWindow.xaml`** - dialog for creating or editing a rental contract.

Each `.xaml` file has a matching `.xaml.cs` code-behind file.

## Technology

- **.NET 8** / C#
- **WPF** for the user interface
- **Entity Framework Core 9** with the **SQLite** provider
