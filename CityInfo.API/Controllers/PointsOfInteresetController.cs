using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInteresetController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            var city = CitiesDataStore.Current.Cities
                .FirstOrDefault(city => city.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            var city = CitiesDataStore.Current.Cities
                .FirstOrDefault(city => city.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterest =
                city.PointsOfInterest.FirstOrDefault(pointOfInterest => pointOfInterest.Id == pointOfInterestId);
            if (pointOfInterest == null)
            {
                return NotFound();
            }

            return Ok(pointOfInterest);
        }

        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId,
            PointOfInterestForCreationDto pointOfInterest)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var maxPointOfInterestId =
                CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            var newPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description,
            };

            city.PointsOfInterest.Add(newPointOfInterest);


            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = newPointOfInterest.Id
                },
                newPointOfInterest);
        }

        [HttpPut("{pointofinterestid}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId,
            PointOfInterestForUpdateDto pointOfInterest)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            ;

            var pointOfInterestFromDb = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
            if (pointOfInterestFromDb == null)
            {
                return NotFound();
            }

            pointOfInterestFromDb.Name = pointOfInterest.Name;
            pointOfInterestFromDb.Description = pointOfInterest.Description;

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
        public ActionResult PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromDb = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
            if (pointOfInterestFromDb == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch =
                new PointOfInterestForUpdateDto()
                {
                    Name = pointOfInterestFromDb.Name,
                    Description = pointOfInterestFromDb.Description,
                };

            /* Here we will take all the properties from JsonPathcDocument<PointOfInterestForUpdateDto> to PointOfInterestForUpdateDto object
            Also we will pass ModelState in order to be able to perform validation on the object. */
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            /* Validate if some error occured during transforming patchDocument properties values to pointOfInterestToPatch values, but it didn't check
            pointOfInterestToPatch fields valiedation. That needs to be performed separately, in the next step */
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Valudate pointOfInterestToPatch fields after all fields are updated from the patchDocument model
            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }
            
            // Update the pointOfInterest we got from the DB with newly generated PointOfInterestForUpdateDto object value
            pointOfInterestFromDb.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromDb.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(city => city.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromDb = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);
            if (pointOfInterestFromDb == null)
            {
                return NotFound();
            }
            
            city.PointsOfInterest.Remove(pointOfInterestFromDb);
            
            return NoContent();
        }
    }
}