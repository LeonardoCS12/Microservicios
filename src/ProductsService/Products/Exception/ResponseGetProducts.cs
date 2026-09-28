using System.Text.Json.Serialization;
using System.Text.Json.Serialization;

public class ResponseGetProducts<T>
{
    public int Total { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Page { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Limit { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] //Para evitar imprimir valores null al serializar la respuesta.
    public object? Sort { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Order { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? IsDeleted { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Products { get; set; }

    // Constructor
    public ResponseGetProducts(int total, object page, object limit, T products, object? sort, object? order,
        object status, object isDeleted, object? type)
    {
        Total = total;
        Page = page;
        Limit = limit;
        Sort = sort;
        Order = order;
        Status = status;
        IsDeleted = isDeleted;
        Type = type;
        Products = products;
    }
}