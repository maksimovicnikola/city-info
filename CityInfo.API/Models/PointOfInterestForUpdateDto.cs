using System.ComponentModel.DataAnnotations;
using static System.String;

namespace CityInfo.API.Models;

public class PointOfInterestForUpdateDto
{
    [Required(ErrorMessage = "The Name field is required")]
    [MaxLength(50)]
    public string Name { get; set; } = Empty;
    [MaxLength(200)]
    public string? Description { get; set; }
}