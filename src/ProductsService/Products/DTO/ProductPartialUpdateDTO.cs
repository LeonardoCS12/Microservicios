using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class ProductPartialUpdateDTO
{
    [MaxLength(10)]
    public string? type { get; set; }

    [MaxLength(255)]
    public string? name { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal? price { get; set; }

    public bool? status { get; set; }
    public string? text { get; set; }


    [MaxLength(8)]
    public string? Product_key { get; set; }

    [MaxLength(200)]
    public string? image_lick { get; set; }

}