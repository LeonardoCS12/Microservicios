using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apitienda.Models;
using apitienda.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

/// <summary >
/// Clase de acceso a datos (DAO) para la entidad <see cref="Usuario"/>.
/// Proporciona métodos para realizar operaciones CRUD en la base de datos.
/// </summary >
public class UsuarioDAO : IUsuarioDAO
{
private readonly DataContext _context;

/// <summary >
/// Inicializa una nueva instancia de la clase <see cref="UsuarioDAO"/> con el
/// contexto de datos proporcionado.
/// </summary >
/// <param name="context">El contexto de la base de datos.</param >
public UsuarioDAO(DataContext context)
{
_context = context;
}

/// <summary >
/// Obtiene todos los productos con sentencia personalizada del cliente (Query).
/// </summary >
public async Task<List<Usuario >> GetUserAsync(string sentencia ,
DynamicParameters parametros)
{

var connection = _context.Database.GetDbConnection();

// Importante: No uses `using ` ni abras manualmente la conexión asi se queda.
return (await connection.QueryAsync <Usuario >(sentencia , parametros)).ToList();
}


/// <summary >
/// Obtiene un usuario por su identificador único de manera asíncrona.
/// </summary >
/// <param name="id">El identificador único del usuario.</param >
/// <param name="includeDeleted">Si es verdadero , también incluye usuarios eliminados.</param >
/// <returns >El usuario correspondiente o <c>null </c> si no se encuentra.</returns >
public async Task<Usuario?> GetByIdAsync(Guid id)
{
return await _context.Users.FirstOrDefaultAsync(u => u.id == id);

}

/// <summary >
/// Agrega un nuevo usuario a la base de datos de manera asíncrona.
/// </summary >
/// <param name="usuario">El usuario a agregar.</param >
/// <returns >Una tarea que representa la operación de inserción.</returns >
public async Task AddAsync(Usuario usuario)
{
try
{
await _context.Users.AddAsync(usuario);
await _context.SaveChangesAsync();
}
catch (Exception ex)
{
Console.WriteLine("Error al guardar usuario: " + ex.Message);
throw; // o log más detallado
}
}

/// <summary >
/// Actualiza un usuario existente en la base de datos de manera asíncrona.
/// </summary >
/// <param name="usuario">El usuario con los datos actualizados.</param >
/// <returns >Una tarea que representa la operación de actualización.</returns >
public async Task UpdateAsync(Usuario usuario)
{
// Verifica si la entidad ya está siendo rastreada.
var usuarioExistente = await _context.Users
.AsNoTracking() // No rastrea esta entidad para evitar conflicto
.FirstOrDefaultAsync(u => u.id == usuario.id);

if (usuarioExistente != null)
{
// Asegúrate de que la entidad no esté siendo rastreada ya por el contexto
_context.Entry(usuarioExistente).State = EntityState.Detached;

// Actualiza la entidad
_context.Users.Update(usuario);
await _context.SaveChangesAsync();
}
}

/// <summary >
/// Marca un usuario como eliminado en la base de datos de manera asíncrona.
/// </summary >
/// <param name="id">El identificador único del usuario a eliminar.</param >
/// <returns >Una tarea que representa la operación de eliminación lógica.</returns >
public async Task DeleteAsync(Guid id)
{
var usuario = await _context.Users.FindAsync(id);
if (usuario != null)
{
usuario.is_deleted = true;
usuario.deleted_at = DateTimeOffset.UtcNow;
_context.Users.Update(usuario);
await _context.SaveChangesAsync();
}
}

/// <summary >
/// Actualiza parcialmente un usuario existente en la base de datos de manera asíncrona.
/// </summary >
/// <param name="id">El identificador único del usuario a actualizar.</param >
/// <param name="usuarioParcial">El usuario con los datos parcialmente actualizados.</param >
/// <returns >Una tarea que representa la operación de actualización parcial.</returns >
public async Task UpdatePartialAsync(Guid id, Usuario usuarioParcial)
{
var usuarioExistente = await _context.Users.FindAsync(id);
if (usuarioExistente != null)
{
if (!string.IsNullOrEmpty(usuarioParcial.first_name))
usuarioExistente.first_name = usuarioParcial.first_name;
if (!string.IsNullOrEmpty(usuarioParcial.last_name))
usuarioExistente.last_name = usuarioParcial.last_name;
if (!string.IsNullOrEmpty(usuarioParcial.email))
usuarioExistente.email = usuarioParcial.email;
// Actualiza otros campos según sea necesario

await _context.SaveChangesAsync();
}
}

/// <summary >
/// Restaura un usuario que ha sido eliminado lógicamente.
/// </summary >
/// <param name="id">El identificador único del usuario.</param >
/// <returns >Una tarea que representa la operación de restauración.</returns >
public async Task<bool > RestoreAsync(Guid id)
{
var usuario = await _context.Users.FirstOrDefaultAsync(u => u.id == id);
if (usuario == null)
{
return false; // Usuario no encontrado
}

usuario.is_deleted = false;
usuario.deleted_at = null;
await _context.SaveChangesAsync();

return true;
}

/// <summary >
/// El método Detach desvincula una entidad del tipo Usuario del DbContext.
/// Verifica si el estado de la entidad usuario no está en el estado Detached.
/// Si no está , entonces cambia su estado a Detached.
/// </summary >
/// <param name="usuario">La entidad Usuario a desvincular del DbContext.</param >
public void Detach(Usuario usuario)
{
if (_context.Entry(usuario).State != EntityState.Detached)
{
_context.Entry(usuario).State = EntityState.Detached;
}
}

/// <summary >
/// Busca un usuario por su email en la base de datos and devuelve el primer
// resultado encontrado o null si no existe.
/// </summary >
/// <param email="email@usuario">La entidad email entra como condicion.</param >

public async Task<Usuario?> GetByEmailAsync(string email)
{
return await _context.Users.FirstOrDefaultAsync(u => u.email == email);
}


/// <summary >
/// Registrar en la tabla passwordresettoken
/// los datos para validar.
/// </summary >
public async Task addResgistroPassword(PasswordResetToken registroPwd)
{
await _context.passwordresettoken.AddAsync(registroPwd);
await _context.SaveChangesAsync();
}


/// <summary >
/// Busca el token de la tabla PasswordResetToken
/// resultado encontrado o null si no existe.
/// </summary >
public async Task<PasswordResetToken?> GetByToken(string token)
{
return await _context.passwordresettoken
.FirstOrDefaultAsync(t => t.Token == token);

}

/// <summary >
/// Busca el token relacionado a un usuario
/// </summary >
public async Task<Usuario?> GetUsuarioToken(string token)
{
return await _context.Users.FirstOrDefaultAsync(e => e.password_reset_token == token );
}

}
