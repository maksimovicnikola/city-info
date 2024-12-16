using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService,
            ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                _logger.LogInformation($"City with id {cityId} was not found when accessing points of interest.");
                return NotFound();
            }

            var pointsOfInterestForCity = await _cityInfoRepository.GetPointsOfInterestAsync(cityId);
            // Map PointOfInterest to PointOfInterestDto via AutoMapper
            var result = _mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);
            
            return Ok(result);
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                _logger.LogInformation($"City with id {cityId} was not found when accessing points of interest.");
                return NotFound();
            }

            var pointsOfInterestForCity =
                await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointsOfInterestForCity == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PointOfInterestDto>(pointsOfInterestForCity));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId,
            PointOfInterestForCreationDto pointOfInterest)
        {
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId,
                    pointOfInterestId = createdPointOfInterestToReturn.Id
                },
                createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
            PointOfInterestForUpdateDto pointOfInterest)
        {
            var cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var pointOfInterestEntity =
                await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            // In case when we want directly to update the destination object instance, we should invoke Map method like this
            // In this way, pointOfInterestEntity properties will be directly overridden
            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        /*
         * Here the request raw body would look like
         * [
         *      {
         *          "op": "replace",
         *          "path": "/name",
         *          "value": "Updated - Central Park"
         *      }
         * ]
         */
        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var cityExist = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExist)
            {
                return NotFound();
            }

            var pointOfInterestEntity =
                await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            /* Here we will take all the properties from JsonPatchDocument<PointOfInterestForUpdateDto> to PointOfInterestForUpdateDto object
            Also we will pass ModelState in order to be able to perform validation on the object. */
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            /* Validate if some error occured during transforming patchDocument properties values to pointOfInterestToPatch values, but it didn't check
            pointOfInterestToPatch fields validation. That needs to be performed separately, in the next step */
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate pointOfInterestToPatch fields after all fields are updated from the patchDocument model
            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            // Update the pointOfInterest we got from the DB with newly generated PointOfInterestForUpdateDto object value
            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            var cityExist = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExist)
            {
                return NotFound();
            }

            var pointOfInterestEntity =
                await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

            return NoContent();
        }
    }
}