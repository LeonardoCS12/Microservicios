using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductDTO{
    [Required]
    public Guid id { get; set; }

    [Required]
    public DateTimeOffset created_at { get; set; }

    [Required]
    public DateTimeOffset modified_at { get; set; }

    [Required]
    public bool is_deleted { get; set; }
    public DateTimeOffset? deleted_at { get; set; }

    [Required]
    [StringLength(10,MinimumLength=1)]
    public required string type { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public required string name { get; set; }

    [Required]
    [Column(TypeName ="decimal(8,2)")]
    public decimal price { get; set; }

    [Required]
    public bool status { get; set; }
    public string? text { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 1)]
    public string? Product_key { get; set; }

    [MaxLength(200)]
    public string? image_link { get; set; }

}