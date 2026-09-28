using System.ComponentModel.DataAnnotations;

public class ProductUpdateImageDTO {
    [MaxLength(200)]
    public string? image_link { get; set; }
}