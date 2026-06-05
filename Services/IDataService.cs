using System.Collections.Generic;
using System.Threading.Tasks;
using CarRentalSystem_WPF.Models;

namespace CarRentalSystem_WPF.Services
{
    public interface IDataService
    {
        Task<List<Car>> GetAllCarsAsync();
        Task<Car?> GetCarByIdAsync(int id);
        Task AddCarAsync(Car car);
        Task UpdateCarAsync(Car car);
        Task DeleteCarAsync(int id);

        Task<List<Renter>> GetAllRentersAsync();
        Task<Renter?> GetRenterByIdAsync(int id);
        Task AddRenterAsync(Renter renter);
        Task UpdateRenterAsync(Renter renter);
        Task DeleteRenterAsync(int id);

        Task<List<Paperwork>> GetAllDocumentsAsync();
        Task<Paperwork?> GetDocumentByIdAsync(int id);
        Task AddDocumentAsync(Paperwork document);
        Task UpdateDocumentAsync(Paperwork document);
        Task DeleteDocumentAsync(int id);

        Task InitializeDatabaseAsync();
        
        Task UpdateExpiredContractsAsync();
    }
}

