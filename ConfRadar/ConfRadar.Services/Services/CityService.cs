using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;

namespace ConfRadar.Services.Services
{
    public interface ICityService
    {
        Task<List<City>> GetAllCitiesAsync();
    }
    public class CityService : ICityService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _unitOfWork.CityRepository.GetAllCitiesAsync();
        }
    }
}
