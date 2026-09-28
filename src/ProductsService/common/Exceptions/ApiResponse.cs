using System.Text.Json.Serialization;
public class ApiResponse <T>
{
public int Codigo { get; set; }
public string Mensaje { get; set; }
//Para evitar imprimir valores null al serializar la respuesta.
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public T? Resultado { get; set; }

// Constructor para cuando se necesita devolver un resultado
public ApiResponse(int codigo , string mensaje , T resultado)
{
Codigo = codigo;
Mensaje = mensaje;
Resultado = resultado;
}
// Constructor para cuando no se necesita devolver un resultado
public ApiResponse(int codigo , string mensaje)
{
Codigo = codigo;
Mensaje = mensaje;
Resultado = default;
}
}
