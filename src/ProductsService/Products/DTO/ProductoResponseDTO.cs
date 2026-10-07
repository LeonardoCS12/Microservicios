public class ProductoResponseDTO
{
    public Guid id { get; set; }
    public required string type { get; set; }
    public required string name { get; set; }
    public decimal price { get; set; }
}