using CarRentalSystem_WPF.Data;
using CarRentalSystem_WPF.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem_WPF.Services
{
    public class DataService : IDataService
    {
        private readonly DatabaseContext _context;

        public DataService()
        {
            _context = new DatabaseContext();
        }

        public async Task InitializeDatabaseAsync()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        // Car methods
        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _context.Cars.ToListAsync();
        }

        public async Task<Car?> GetCarByIdAsync(int id)
        {
            return await _context.Cars.FindAsync(id);
        }

        public async Task AddCarAsync(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCarAsync(Car car)
        {
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCarAsync(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
        }

        // Renter methods
        public async Task<List<Renter>> GetAllRentersAsync()
        {
            return await _context.Renters.ToListAsync();
        }

        public async Task<Renter?> GetRenterByIdAsync(int id)
        {
            return await _context.Renters.FindAsync(id);
        }

        public async Task AddRenterAsync(Renter renter)
        {
            _context.Renters.Add(renter);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRenterAsync(Renter renter)
        {
            _context.Renters.Update(renter);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRenterAsync(int id)
        {
            var renter = await _context.Renters.FindAsync(id);
            if (renter != null)
            {
                _context.Renters.Remove(renter);
                await _context.SaveChangesAsync();
            }
        }

        // Document methods
        public async Task<List<Paperwork>> GetAllDocumentsAsync()
        {
            return await _context.Paperworks
                .Include(p => p.Car)
                .Include(p => p.Renter)
                .ToListAsync();
        }

        public async Task<Paperwork?> GetDocumentByIdAsync(int id)
        {
            return await _context.Paperworks
                .Include(p => p.Car)
                .Include(p => p.Renter)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddDocumentAsync(Paperwork document)
        {
            _context.Paperworks.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDocumentAsync(Paperwork document)
        {
            _context.Paperworks.Update(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var document = await _context.Paperworks
                .Include(paperwork => paperwork.Car)
                .FirstOrDefaultAsync(paperwork => paperwork.Id == id);
            if (document != null)
            {
                // Free the car when document is deleted
                if (document.Car != null)
                {
                    document.Car.IsAvailable = true;
                    _context.Cars.Update(document.Car);
                }
                
                _context.Paperworks.Remove(document);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Checks all contracts and updates car availability for expired contracts.
        /// Sets IsAvailable = true for cars whose rental contracts have expired (RentalEndDate <= today).
        /// This ensures cars become available again automatically when contracts expire.
        /// </summary>
        public async Task UpdateExpiredContractsAsync()
        {
            var today = DateTime.Today;
            
            // Find all expired contracts (where end date has passed or is today)
            // Select only the CarId and Car to avoid issues with ignored properties
            var expiredContractData = await _context.Paperworks
                .Where(paperwork => paperwork.RentalEndDate <= today)
                .Select(paperwork => new { paperwork.CarId, paperwork.Car })
                .ToListAsync();

            // Get unique car IDs that need to be updated
            var carIdsToUpdate = expiredContractData
                .Where(expiredPaperwork => expiredPaperwork.Car != null && !expiredPaperwork.Car.IsAvailable)
                .Select(expiredPaperwork => expiredPaperwork.CarId)
                .Distinct()
                .ToList();

            // Update cars from expired contracts to be available
            if (carIdsToUpdate.Count > 0)
            {
                var carsToUpdate = await _context.Cars
                    .Where(car => carIdsToUpdate.Contains(car.Id) && !car.IsAvailable)
                    .ToListAsync();

                foreach (var car in carsToUpdate)
                {
                    car.IsAvailable = true;
                    _context.Cars.Update(car);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}

