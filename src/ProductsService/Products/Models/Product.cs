using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace productos.Models
{
    public class Product
    {
        [Required]
        public Guid id { get; set; } //= Guid.NewGuid();

        [Required]
        public DateTimeOffset created_at { get; set; } //= DateTimeOffset.Now;

        [Required]
        public DateTimeOffset modified_at { get; set; } //= DateTimeOffset.Now;

        [Required]
        public bool is_deleted { get; set; } //= false;

        public DateTimeOffset? deleted_at { get; set; }

        [MaxLength(10)]
        [Required]
        public string type { get; set; } = string.Empty;

        [MaxLength(255)]
        [Required]
        public string name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(8,2)")] // Define la precisión y escala del precio en la base de datos.
        public decimal price { get; set; } = 0;

        [Required]
        public bool status { get; set; } = false;

        public string? text { get; set; }

        [MaxLength(8)]
        [Required]
        public string? Product_key { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? image_lick { get; set; }
    }
}