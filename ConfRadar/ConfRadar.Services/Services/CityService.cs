using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
