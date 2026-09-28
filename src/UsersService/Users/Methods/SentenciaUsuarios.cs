using Dapper;

/// <summary >
/// Clase encargada de construir sentencias SQL dinámicas para la entidad <c>Users
/// </c>.
/// Permite generar consultas filtradas , paginadas y ordenadas , así como validar
/// existencia de email o username.
/// </summary >
public class SentenciaUsuarios
{
// Parámetros opcionales para filtros y paginación
int? safePage;
int? safeLimit;
string? sort;
string? safeOrder;
bool? safeActive;
bool? safeisDeleted;
bool? safeIsSuperuser;
bool? safeEmailVerified;

// Variables para los valores la validacion de emal y usuario
private string? Email;
private string? Username;

object[] valores = new object[8];

public string sentencia = "SELECT * FROM \"Users\" WHERE 1=1 ";

// Constructor para la sentencias Query
public SentenciaUsuarios(int? safePage , int? safeLimit , string? sort, string?
safeOrder , bool? safeActive , bool? safeisDeleted , bool? safeIsSuperuser ,
bool? safeEmailVerified)
{
this.safePage = safePage;
this.safeLimit = safeLimit;
this.sort = sort;
this.safeOrder = safeOrder;
this.safeActive = safeActive;
this.safeisDeleted = safeisDeleted;
this.safeIsSuperuser = safeIsSuperuser;
this.safeEmailVerified = safeEmailVerified;
}
// Constructor para la validacion de email y username
public SentenciaUsuarios(string email ,string username)
{
Email = email;
Username = username;
}
// Método para crear la sentencia SQL Query
public (string Sentencia , DynamicParameters Parametros ,object[] valores)
CrearSenentiaSQLUser()
{
var parametros = new DynamicParameters();

if(safeActive.HasValue)
{
sentencia += " AND is_active = @active ";
parametros.Add("@active", safeActive);
valores[0] = safeActive;
}

if(safeisDeleted.HasValue)
{
sentencia += " AND is_deleted = @is_deleted ";
parametros.Add("@is_deleted", safeisDeleted);
valores[1] = safeisDeleted;
}

if(safeIsSuperuser.HasValue)
{
sentencia += " AND is_superuser = @is_superuser ";
parametros.Add("@is_superuser", safeIsSuperuser);
valores[2] = safeIsSuperuser;
}

if(safeEmailVerified.HasValue)
{
sentencia += " AND email_verified = @email_verified ";
parametros.Add("@email_verified", safeEmailVerified);
valores[3] = safeEmailVerified;
}

if (!string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(safeOrder) )
{
sentencia += $" ORDER BY @sort @order";
parametros.Add("@sort", sort);
parametros.Add("@order", safeOrder);
valores[4] = sort;
valores[5] = safeOrder;
}

if(safeLimit.HasValue && safePage.HasValue )
{
// Paginación
sentencia += " LIMIT @limit OFFSET @offset ";
parametros.Add("@limit", safeLimit);
parametros.Add("@offset", (safePage - 1) * safeLimit);
valores[6] = safeLimit;
valores[7] = safePage;
}

return (sentencia , parametros ,valores);
}
// Método para crear la sentencia SQL para validar email y username
public (string Sentencia , DynamicParameters Parametros)
CrearSentenciaSQLValidarEmailUsername()
{
var parametros = new DynamicParameters();
sentencia += " AND (username = @username OR email = @email) ";
parametros.Add("@username", Username);
parametros.Add("@email", Email);
Console.WriteLine("username: "+Username);
Console.WriteLine("Email: "+Email);

return (sentencia , parametros);
}

}
