using System.Text.Json.Serialization;

/// <summary >
/// Clase genérica para la respuesta de usuarios paginada y filtrada.
/// Contiene información de total de registros , paginación , filtros aplicados y la
/// lista de usuarios.
/// </summary >
/// <typeparam name="T">Tipo de los objetos de usuario que se incluirán en la
/// respuesta.</typeparam >
public class ResponseGetUsers<T>
{
    /// <summary >
    /// Total de usuarios disponibles según los filtros aplicados.
    /// </summary >
    public int Total { get; set; }

    /// <summary >
    /// Página actual. Se ignora si es null al serializar.
    /// </summary >
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] //Para evitar imprimir valores null al serializar la respuesta.
    public object? Page { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Limit { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Sort { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Order { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? is_active { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? is_deleted { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? is_superuser { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? email_verified { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Users { get; set; }

    /// <summary >
    /// Constructor que inicializa todos los campos de la respuesta.
    /// </summary >
    public ResponseGetUsers(int total, object? page, object? limit, object? sort,
    object? order, object? is_active, object? is_deleted, object? is_superuser, 
    object? email_verified, T? users)
    {
        Total = total;
        Page = page;
        Limit = limit;
        Sort = sort;
        Order = order;
        this.is_active = is_active;
        this.is_deleted = is_deleted;
        this.is_superuser = is_superuser;
        this.email_verified = email_verified;
        Users = users;
    }
}
