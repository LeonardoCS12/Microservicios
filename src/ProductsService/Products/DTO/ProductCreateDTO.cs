public class ProductCreateDTO
{
    public string type { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? text { get; set; }
    public string? image_link { get; set; }
    public string? Product_key { get; set; }
}