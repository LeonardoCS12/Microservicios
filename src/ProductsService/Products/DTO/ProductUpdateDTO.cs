using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductUpdateDTO{
    [Required]
    [StringLength(10,MinimumLength =1)]
    public required string type { get; set; }

    [Required]
    [StringLength(255, MinimumLength = 1)]
    public required string name { get; set; }

    [Required]
    [Column(TypeName = "decimal(8,2)")]
    public decimal price { get; set; }

    [Required]
    public bool status { get; set; }
    public string? text { get; set; }

    [MaxLength(8)]
    public string? Product_key { get; set; }

    [MaxLength(200)]
    public string? image_link { get; set; }
}