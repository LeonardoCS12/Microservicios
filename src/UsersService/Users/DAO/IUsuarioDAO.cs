using Dapper;
using apitienda.Models;
public interface IUsuarioDAO
{
/// <summary >
/// Obtiene una lista de usuarios según una consulta SQL dinámica.
/// </summary >
/// <param name="sentencia">Consulta SQL a ejecutar.</param >
/// <param name="parametros">Parámetros dinámicos para la consulta.</param >
/// <returns >Una lista de usuarios que coincidan con la consulta.</returns >
Task<List<Usuario >> GetUserAsync(string sentencia , DynamicParameters parametros);

/// <summary >
/// Obtiene un usuario por su ID.
/// </summary >
/// <param name="id">Identificador único del usuario.</param >
/// <param name="includeDeleted">Indica si se deben incluir usuarios eliminados.</param >
/// <returns >El usuario encontrado o null si no existe.</returns >
Task<Usuario?> GetByIdAsync(Guid id);

/// <summary >
/// Agrega un nuevo usuario a la base de datos.
/// </summary >
/// <param name="usuario">Objeto usuario a agregar.</param >
/// <returns >Tarea asincrónica sin valor de retorno.</returns >
Task AddAsync(Usuario usuario);

/// <summary >
/// Actualiza la información de un usuario existente.
/// </summary >
/// <param name="usuario">Objeto usuario con los datos actualizados.</param >
/// <returns >Tarea asincrónica sin valor de retorno.</returns >
Task UpdateAsync(Usuario usuario);

/// <summary >
/// Elimina un usuario por su ID.
/// </summary >
/// <param name="id">Identificador único del usuario a eliminar.</param >
/// <returns >Tarea asincrónica sin valor de retorno.</returns >
Task DeleteAsync(Guid id);

/// <summary >
/// Actualiza parcialmente la información de un usuario.
/// </summary >
/// <param name="id">Identificador único del usuario.</param >
/// <param name="usuarioParcial">Objeto con los datos a actualizar.</param >
/// <returns >Tarea asincrónica sin valor de retorno.</returns >
Task UpdatePartialAsync(Guid id, Usuario usuarioParcial);

/// <summary >
/// Restaura un usuario eliminado previamente.
/// </summary >
/// <param name="id">Identificador único del usuario a restaurar.</param >
/// <returns >Retorna true si la restauración fue exitosa , false en caso contrario.</returns >
Task<bool > RestoreAsync(Guid id);

/// <summary >
/// Obtiene un usuario por su dirección de correo electrónico.
/// </summary >
/// <param name="email">Correo electrónico del usuario.</param >
/// <returns >El usuario encontrado o null si no existe.</returns >
Task<Usuario?> GetByEmailAsync(string email);

/// <summary >
/// Desvincula una instancia de usuario del contexto de seguimiento de Entity Framework.
/// </summary >
/// <param name="usuario">Objeto usuario a desvincular.</param >
void Detach(Usuario usuario);


/// <summary >
/// Busca un token de restablecimiento de contraseña por su valor.
/// </summary >
Task<PasswordResetToken?> GetByToken(string token);


/// <summary >
/// Registramos los datos en la tabla PasswordResetToken
/// </summary >
Task addResgistroPassword(PasswordResetToken registroPwd);

/// <summary >
/// Buscamos un token relacionado a un usuario
/// si no lo encuenta regresamos null.
/// </summary >
Task<Usuario?> GetUsuarioToken(string token);

}
