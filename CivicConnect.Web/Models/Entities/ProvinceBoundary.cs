using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CivicConnect.Web.Models.Entities
{
    public class ProvinceBoundary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string ProvinceCode { get; set; }

        [Required]
        [StringLength(100)]
        public string ProvinceName { get; set; }

        // Bounding Box Coordinates
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLng { get; set; }
        public double MaxLng { get; set; }
    }
}
