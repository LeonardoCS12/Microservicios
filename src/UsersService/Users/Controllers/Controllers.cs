using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apitienda.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.Text.RegularExpressions;

/// <summary>
/// Controlador para gestionar las operaciones relacionadas con los usuarios.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class usersController : ControllerBase
{
    private readonly IUsuarioService _iUsuarioService;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UsuarioController"/>.
    /// </summary>
    public usersController(IUsuarioService iUsuarioService)
    {
        _iUsuarioService = iUsuarioService;
    }

    /// <summary>
    /// Obtiene la lista de usuarios con opciones de paginación, ordenamiento y filtrado.
    /// </summary>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="limit">Cantidad de registros por página.</param>
    /// <param name="sort">Campo por el cual se ordenarán los resultados.</param>
    /// <param name="order">Orden de los resultados: "asc" o "desc".</param>
    /// <param name="is_active">Filtra usuarios activos.</param>
    /// <param name="is_deleted">Filtra usuarios eliminados.</param>
    /// <param name="is_superuser">Filtra usuarios con privilegios de superusuario.</param>
    /// <param name="email_veridied">Filtra usuarios con correo electrónico verificado.</param>
    /// <returns>Lista de usuarios que cumplen con los criterios especificados.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllUsersAsync(int? page, int? limit, string? sort,
        string? order, bool? is_active, bool? is_deleted, bool? is_superuser, bool? email_veridied)
    {
        try
        {
            // Validación de la página: debe ser mayor a 0 si se especifica
            if (!(page == null))
            {
                if (page < 1)
                {
                    return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
                }
            }

            // Validación del límite de resultados por página
            if (!(limit == null))
            {
                if (limit < 1)
                {
                    return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
                }
            }
            // Validación del tipo de ordenamiento (ascendente o descendente)
            if (!(order == null))
            {
                if (!(order == "asc" || order == "desc"))
                {
                    return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
                }
            }
            // Validación del campo permitido para ordenar los resultados
            if (!(sort == null))
            {
                if (!(sort == "username" || sort == "email" || sort == "date_joined"
                    || sort == "first_name" || sort == "last_name"
                    || sort == "is_active" || sort == "is_deleted" || sort == "is_superuser"
                    || sort == "email_verified" || sort == "date_of_birth"
                    || sort == "nationality" || sort == "occupation" || sort == "gender" || sort == "role"))
                {
                    return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
                }
            }
            // Llamada al servicio que obtiene los usuarios aplicando los filtros y la paginación
            var usuarios = await _iUsuarioService.GetAllAsync2(page, limit, sort, order, is_active, is_deleted, is_superuser, email_veridied);
            // Retorna la lista de usuarios obtenida desde el servicio
            return usuarios;
        }
        catch (Exception ex)
        {
            // Registro del error en consola para depuración
            Console.WriteLine("Error 500: " + ex);

            // Retorna error interno del servidor
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Obtiene un usuario por su identificador único.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            // Validación del identificador: no debe ser un GUID vacío
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Llama al servicio para obtener el usuario por su ID
            var usuario = await _iUsuarioService.GetByIdAsync(id);

            // Retorna la respuesta obtenida desde el servicio
            return usuario;
        }
        catch (Exception ex)
        {
            // Registro del error en consola para fines de depuración
            Console.WriteLine("Error 500: " + ex);
            // Retorna un error interno del servidor
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }

    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    /// <param name="usuarioCreateDTO">
    /// Objeto que contiene la información necesaria para registrar un usuario.
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UsuarioCreateDTO usuarioCreateDTO)
    {
        try
        {
            // Validación: verifica que el objeto recibido en el cuerpo de la solicitud no sea nulo
            if (usuarioCreateDTO == null)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controllerPostUser")));
            }
            // Validación del modelo según las reglas definidas en el DTO (DataAnnotations)
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controllerPostUser")));
            }

            /// <summary>
            /// verifica si el email tiene una correcta sixtaxis.
            /// </summary>
            var email = usuarioCreateDTO.email;
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (!emailRegex.IsMatch(email))
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controllerPostUser")));
            }
            var usuario = await _iUsuarioService.AddAsync(usuarioCreateDTO);
            return usuario;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }

    }

    /// <summary>
    /// Actualiza un usuario existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UsuarioUpdate usuarioUpdate)
    {
        try
        {
            // Validación: verifica que el objeto recibido en el cuerpo de la solicitud no sea nulo
            if (usuarioUpdate == null)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Validación del modelo de datos según las reglas definidas en el DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Llama al servicio encargado de actualizar la información del usuario
            var updatedUser = await _iUsuarioService.UpdateAsync(id, usuarioUpdate);
            // Retorna la respuesta generada por el servicio
            return updatedUser;
        }
        catch (Exception ex)
        {
            // Registro del error en consola para fines de depuración
            Console.WriteLine("Error 500: " + ex);
            // Retorna una respuesta de error interno del servidor
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Elimina un usuario por su identificador único.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }

            var existingUser = await _iUsuarioService.DeleteAsync(id);
            return existingUser;

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }

    }

    /// <summary>
    /// Actualiza parcialmente un usuario existente.
    /// </summary>
    /// <param name="id">Identificador único del usuario que se desea modificar.</param>
    /// <param name="usuarioDTO">
    /// Objeto que contiene únicamente los campos del usuario que se desean actualizar.
    /// </param>
    /// <returns>Respuesta HTTP con el resultado de la actualización parcial del usuario.</returns>
    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UsuarioPatchDTO usuarioDTO)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Validación: verifica que el objeto recibido en la solicitud no sea nulo
            if (usuarioDTO == null)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Llama al servicio encargado de realizar la actualización parcial del usuario
            var existingUser = await _iUsuarioService.UpdatePartialAsync(id, usuarioDTO);
            // Retorna la respuesta generada por el servicio
            return existingUser;
        }
        catch (Exception ex)
        {
            // Registro del error en consola para facilitar la depuración
            Console.WriteLine("Error 500: " + ex);
            // Retorna una respuesta de error interno del servidor
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Restaura un usuario eliminado.
    /// </summary>
    [HttpPatch("{id}/restore")]
    public async Task<IActionResult> RestoreUserAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }

            var usuarioRestaurado = await _iUsuarioService.RestoreUserAsync(id);
            return usuarioRestaurado;

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500:" + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Endpoint HTTP POST para verificar el correo electrónico de un usuario.
    /// </summary>
    [HttpPost("{id}/verify-email")]
    public async Task<IActionResult> VerifyEmail(Guid id)
    {
        try
        {
            var usuarioVerificado = await _iUsuarioService.VerifyEmailAsync(id);
            return usuarioVerificado;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Endpoint HTTP PATCH para activar un usuario estableciendo el campo is_active en true.
    /// </summary>
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {

            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }

            var activarUsuario = await _iUsuarioService.ActiveteUserAsync(id);
            return activarUsuario;

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Desactiva un usuario estableciendo is_active en false.
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }

            var usuario = await _iUsuarioService.DeactivateUserAsync(id);
            return usuario;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Permite cambiar la contraseña de un usuario específico.
    /// </summary>
    /// <param name="id">
    /// Identificador único del usuario cuya contraseña se desea modificar.
    /// </param>
    /// <param name="model">
    /// Objeto que contiene la información necesaria para el cambio de contraseña
    /// (por ejemplo: contraseña actual y nueva contraseña).
    /// </param>
    /// <returns>
    /// Respuesta HTTP con el resultado de la operación de cambio de contraseña.
    /// </returns>
    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDTO model)
    {

        try
        {

            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Validación del modelo recibido según las reglas definidas en el DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Llama al servicio encargado de cambiar la contraseña del usuario
            var result = await _iUsuarioService.ChangePasswordAsync(id, model);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }

    }

    /// <summary>
    /// Solicita el restablecimiento de contraseña enviando un enlace al correo del usuario.
    /// </summary>
    [HttpPost("request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDTO request)
    {
        try
        {
            // Validación: verifica que el objeto recibido no sea nulo
            // y que el correo electrónico esté presente
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Llama al servicio encargado de generar la solicitud
            // de restablecimiento de contraseña
            var respuesta = await _iUsuarioService.RequestPasswordResetAsync(request.Email);
            // Retorna la respuesta generada por el servicio
            return respuesta;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex);
            // Retorna una respuesta de error interno del servidor
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

    /// <summary>
    /// Restablece la contraseña de un usuario utilizando un token de restablecimiento.
    /// </summary>
    ///
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO request)
    {
        try
        {
            // Validación: verifica que el objeto recibido no sea nulo
            // y que contenga el token y la nueva contraseña
            if (request == null || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new ApiResponse<string>(400, MessageService.Instance.GetMessage("controller400")));
            }
            // Llama al servicio encargado de validar el token
            // y actualizar la contraseña del usuario
            var respuesta = await _iUsuarioService.ResetPasswordAsync(request.Token, request.NewPassword);
            // Retorna la respuesta generada por el servicio
            return respuesta;
        }
        catch (Exception ex)
        {
            // Registro del error en consola para fines de depuración
            Console.WriteLine("Error 500: " + ex);
            // Retorna una respuesta de error interno del servidor
            return StatusCode(500, new ApiResponse<string>(500, MessageService.Instance.GetMessage("controllerUser500")));
        }
    }

}