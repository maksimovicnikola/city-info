using CityInfo.API.Entities;

namespace CityInfo.API.Services;

public interface ICityInfoRepository
{
    Task<IEnumerable<City>> GetCitiesAsync();
    Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
    Task<PointOfInterest?> GetPointOfInterestAsync(int cityId, int pointOfInterestId);
    Task<IEnumerable<PointOfInterest>> GetPointsOfInterestAsync(int cityId);
}