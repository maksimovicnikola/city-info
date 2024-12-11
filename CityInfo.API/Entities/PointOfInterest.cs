using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities;

public class PointOfInterest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [ForeignKey("CityId")]
    public City City { get; set; } // Navigation property; Reference type, SqlLite will make a relationship
    public int CityId { get; set; } // Foreign key, this is a primary key of a City entity, not required to be here but it is a recommendation. Without this field, still the CityId would be used as a Foreign key

    public PointOfInterest(string name)
    {
        Name = name;
    }
}