using Microsoft.AspNetCore.Mvc;
using apitienda.Data;
using apitienda.Models;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioDAO _iUsuarioDAO;
    private readonly UsuarioMapper _usuarioMapper;
    private readonly CreateUserMapper _createUserMapper;
    private readonly PasswordResetEmail _emailService; // Inyectar EmailService
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _ihttpContextAccessor;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="UsuarioService"/>.
    /// </summary>
    /// <param name="usuarioDAO">El DAO para la interacción con la base de datos.</param>
    /// <param name="usuarioMapper">El mapper para convertir entre entidades y DTOs.</param>
    public UsuarioService(IHttpContextAccessor contextAccessor, DataContext context, IUsuarioDAO iUsuarioDAO, UsuarioMapper usuarioMapper,
        CreateUserMapper createUserMapper, PasswordResetEmail emailService)
    {
        _iUsuarioDAO = iUsuarioDAO;
        _usuarioMapper = usuarioMapper;
        _createUserMapper = createUserMapper;
        _emailService = emailService; // Asignar en el constructor
        _context = context;
        _ihttpContextAccessor = contextAccessor;
    }

    // 7.9.1 Servicio: obtener todos los usuarios
    /// <summary>
    /// Obtiene todos los usuarios de manera asíncrona.
    /// Obtiene una lista de usarios donde tengamos que pasarle parametros
    /// </summary>
    /// <returns>Una lista de objetos <see cref="UsuarioDTO"/>.</returns>
    public async Task<IActionResult> GetAllAsync2(int? safePage, int? safeLimit, string? sort,
        string? safeOrder, bool? safeActive, bool? safeisDeleted, bool? safeIsSuperuser,
        bool? safeEmailVerified)
    {
        if (!safePage.HasValue && !safeLimit.HasValue && string.IsNullOrEmpty(sort)
            && string.IsNullOrEmpty(safeOrder) && !safeActive.HasValue && !safeisDeleted.HasValue
            && !safeIsSuperuser.HasValue && !safeEmailVerified.HasValue)
        {
            safeisDeleted = false;
        }

        SentenciaUsuarios Crearsentencia = new SentenciaUsuarios(safePage, safeLimit, sort,
            safeOrder, safeActive, safeisDeleted, safeIsSuperuser, safeEmailVerified);

        var sentencia = Crearsentencia.CrearSenentiaSQLUser();
        var usuarios = await _iUsuarioDAO.GetUserAsync(sentencia.Sentencia, sentencia.Parametros);

        if (usuarios == null || !usuarios.Any())
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("getAllUser404")));
        }

        var numResultado = usuarios.Count();
        var resultado = usuarios.Select(u => _usuarioMapper.ToDTO(u)).ToList();

        /// <summary>
        /// Retornamos un ok 200 y un mensaje de éxito.
        /// Utilizamos ApiResponse para estructurar la respuesta.
        /// La respuesta incluye un objeto ResponseGetUsers que contiene información adicional
        /// sobre la paginación y los filtros aplicados.
        /// </summary>
        return new OkObjectResult(new ApiResponse<ResponseGetUsers<List<UsuarioDTO>>>(200,
            MessageService.Instance.GetMessage("getAlluser200"),
            new ResponseGetUsers<List<UsuarioDTO>>(
                numResultado,               // Total de usuarios
                sentencia.valores[7],       // Valor correspondiente a la paginación
                sentencia.valores[6],       // Valor correspondiente al límite
                sentencia.valores[4],       // Valor correspondiente al sort
                sentencia.valores[5],       // Valor correspondiente al safeOrder
                sentencia.valores[0],       // Valor correspondiente a la actividad
                sentencia.valores[1],       // Valor correspondiente a la eliminación
                sentencia.valores[2],       // Valor correspondiente al superusuario
                sentencia.valores[3],       // Valor correspondiente a la verificación del correo
                resultado                   // Lista de usuarios
            )));
    }

    // 7.9.2 Servicio: activar un usuario
    /// <summary>
    /// Acviva un usuario que este desactivado.
    /// </summary>
    /// <param name="id">El identificador único del usuario.</param>
    /// <returns>Un objeto <see cref="UsuarioDTO"/> o <c>null</c> si no se encuentra.</returns>
    public async Task<IActionResult> ActiveteUserAsync(Guid id)
    {
        var usuario = await _iUsuarioDAO.GetByIdAsync(id);

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("ActivateUserUser404")));
        }

        if (usuario.is_active)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("ActivateUserUser400")));
        }

        usuario.is_active = true;

        var usuarioActivado = _usuarioMapper.ToDTO(usuario);

        await _iUsuarioDAO.UpdateAsync(usuario);

        var resultado = new UsuarioDTOResponce(id, usuarioActivado);

        return new OkObjectResult(new ApiResponse<UsuarioDTOResponce>(200,
            MessageService.Instance.GetMessage("ActivateUserUser200"), resultado));
    }

    // 7.9.3 Servicio: obtener un usuario por ID
    /// <summary>
    /// Obtiene un usuario por su ID de manera asíncrona.
    /// </summary>
    /// <param name="id">El identificador único del usuario.</param>
    /// <returns>Un objeto <see cref="UsuarioDTO"/> o <c>null</c> si no se encuentra.</returns>
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var usuario = await _iUsuarioDAO.GetByIdAsync(id);

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("GetByIdAsyncUser404")));
        }

        var resultado = _usuarioMapper.ToDTO(usuario);
        return new OkObjectResult(new ApiResponse<UsuarioDTO>(200,
            MessageService.Instance.GetMessage("GetByIdAsyncUser200"), resultado));
    }

    // 7.9.4 Servicio: crear un nuevo usuario
    /// <summary>
    /// Agrega un nuevo usuario de manera asíncrona.
    /// </summary>
    /// <param name="usuarioCreateDTO">Los datos del usuario en formato DTO.</param>
    /// <returns>Una tarea que representa la operación de inserción.</returns>
    public async Task<IActionResult> AddAsync(UsuarioCreateDTO usuarioCreateDTO)
    {
        SentenciaUsuarios Crearsentencia = new SentenciaUsuarios(usuarioCreateDTO.email,
            usuarioCreateDTO.username);
        var sentencia = Crearsentencia.CrearSentenciaSQLValidarEmailUsername();
        var existeUsuario = await _iUsuarioDAO.GetUserAsync(sentencia.Sentencia, sentencia.Parametros);

        if (!(existeUsuario == null || !existeUsuario.Any()))
        {
            return new ObjectResult(new ApiResponse<string>(409,
                MessageService.Instance.GetMessage("AddAsyncUser409")))
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }

        var usuario = _createUserMapper.ToEntity(usuarioCreateDTO);
        var usuarioDTO = _usuarioMapper.ToDTO(usuario);

        await _iUsuarioDAO.AddAsync(usuario);

        try
        {
            var adminUser = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = adminUser?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? adminUser?.Identity?.Name
                          ?? "SISTEMA/REGISTRO_PUBLICO",
                Accion = "CREAR_USUARIO",
                Tabla = "Users",
                RegistroId = usuario.id.ToString(),
                Detalle = $"Se creó el nuevo usuario: {usuario.username} con email: {usuario.email}",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al auditar creación de usuario: {ex.Message}");
        }

        var resultado = new UsuarioDTOResponceExtends(usuario.id, usuarioDTO);
        return new CreatedResult("", new ApiResponse<UsuarioDTOResponceExtends>(201,
            MessageService.Instance.GetMessage("AddAsyncUser201"), resultado));
    }

    // 7.9.5 Servicio: actualizar un usuario (completo)
    /// <summary>
    /// Actualiza un usuario existente de manera asíncrona.
    /// </summary>
    /// <param name="usuarioDTO">Los datos del usuario en formato DTO.</param>
    /// <returns>Una tarea que representa la operación de actualización.</returns>
    public async Task<IActionResult> UpdateAsync(Guid id, UsuarioUpdate usuarioUpdate)
    {
        var usuario = await _iUsuarioDAO.GetByIdAsync(id); // Convierte el DTO en una entidad.

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("UpdateAsyncUser404")));
        }

        if (usuario.is_deleted)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("UpdateAsyncUser400Deleted")));
        }

        if (usuario.is_active == false)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("UpdateAsyncUser400Active")));
        }

        string usernamePrevio = usuario.username;

        // Verifica si el correo electrónico ya está en uso por otro usuario
        SentenciaUsuarios Crearsentencia = new SentenciaUsuarios(usuarioUpdate.email,
            usuarioUpdate.username);
        var sentencia = Crearsentencia.CrearSentenciaSQLValidarEmailUsername();
        var existeUsuario = await _iUsuarioDAO.GetUserAsync(sentencia.Sentencia, sentencia.Parametros);

        if (!(existeUsuario == null || !existeUsuario.Any()))
        {
            return new ObjectResult(new ApiResponse<string>(409,
                MessageService.Instance.GetMessage("UpdateAsyncUser409")))
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }

        // Actualizamos los campos que nos proporciono el usuario
        usuario.username = usuarioUpdate.username;
        usuario.password = usuarioUpdate.password;
        usuario.email = usuarioUpdate.email;
        usuario.first_name = usuarioUpdate.first_name;
        usuario.last_name = usuarioUpdate.last_name;
        usuario.is_active = usuarioUpdate.is_active;
        usuario.is_superuser = usuarioUpdate.is_superuser;
        usuario.profile_picture = usuarioUpdate.profile_picture;
        usuario.nationality = usuarioUpdate.nacionality;
        usuario.occupation = usuarioUpdate.occupation;
        usuario.date_of_birth = usuarioUpdate.date_of_birth;
        usuario.contact_phone_number = usuarioUpdate.contact_phone_number;
        usuario.gender = usuarioUpdate.gender;
        usuario.address = usuarioUpdate.address;
        usuario.address_number = usuarioUpdate.address_number;
        usuario.address_interior_number = usuarioUpdate.address_interior_number;
        usuario.address_complement = usuarioUpdate.address_complement;
        usuario.address_neighborhood = usuarioUpdate.address_neighborhood;
        usuario.address_zip_code = usuarioUpdate.address_zip_code;
        usuario.address_city = usuarioUpdate.address_city;
        usuario.address_state = usuarioUpdate.address_state;
        usuario.role = usuarioUpdate.role;

        // Actualiza la fecha de modificación
        usuario.modified_at = DateTimeOffset.UtcNow;

        _iUsuarioDAO.Detach(usuario);
        await _iUsuarioDAO.UpdateAsync(usuario);

        try
        {
            var editorUser = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = editorUser?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? editorUser?.Identity?.Name
                          ?? "SISTEMA/AUTO_UPDATE",
                Accion = "ACTUALIZAR_USUARIO",
                Tabla = "Users",
                RegistroId = id.ToString(),
                Detalle = $"Actualización de datos para el usuario: {usernamePrevio}. " +
                          $"Nuevo username: {usuario.username}, Rol: {usuario.role}, Activo: {usuario.is_active}",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al auditar actualización de usuario {id}: {ex.Message}");
        }

        // Convierte la entidad actualizada a DTO
        var usuarioDTOResponce = _usuarioMapper.ToDTO(usuario); // Convierte la entidad actualizada a DTO
        var resultado = new UsuarioDTOResponceExtends(usuario.id, usuarioDTOResponce);

        return new OkObjectResult(new ApiResponse<UsuarioDTOResponceExtends>(200,
            MessageService.Instance.GetMessage("UpdateAsyncUser200"), resultado));
    }

    // 7.9.6 Servicio: eliminar un usuario (borrado lógico)
    /// <summary>
    /// Elimina un usuario por su ID de manera asíncrona.
    /// </summary>
    /// <param name="id">El identificador único del usuario a eliminar.</param>
    /// <returns>Una tarea que representa la operación de eliminación.</returns>
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var existeusuario = await _iUsuarioDAO.GetByIdAsync(id);
        if (existeusuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("controllerUser404")));
        }
        if (existeusuario.is_deleted)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("DeleteAsyncUser400")));
        }

        string usuarioEliminado = existeusuario.username;
        string emailEliminado = existeusuario.email;

        await _iUsuarioDAO.DeleteAsync(id);

        try
        {
            var adminUser = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = adminUser?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? adminUser?.Identity?.Name
                          ?? "SISTEMA/ADMIN",
                Accion = "ELIMINAR_USUARIO",
                Tabla = "Users",
                RegistroId = id.ToString(),
                Detalle = $"Se eliminó al usuario: {usuarioEliminado} (Email: {emailEliminado})",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error crítico de auditoría al eliminar usuario {id}: {ex.Message}");
        }

        return new OkObjectResult(new ApiResponse<string>(200,
            MessageService.Instance.GetMessage("DeleteAsyncUser200")));
    }

    // 7.9.7 Servicio: actualizar un usuario (parcial)
    /// <summary>
    /// Actualiza parcialmente un usuario existente de manera asíncrona.
    /// </summary>
    /// <param name="id">El identificador único del usuario a actualizar.</param>
    /// <param name="usuarioDTO">Los datos del usuario en formato DTO.</param>
    /// <returns>Una tarea que representa la operación de actualización parcial.</returns>
    public async Task<IActionResult> UpdatePartialAsync(Guid id, UsuarioPatchDTO usuarioDTO)
    {
        var existingUser = await _iUsuarioDAO.GetByIdAsync(id);

        if (existingUser == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("UpdatePartialAsyncUser404")));
        }

        if (existingUser.is_deleted)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("UpdatePartialAsyncUser400")));
        }

        string infoPrevia = $"Usuario: {existingUser.username} (Email: {existingUser.email})";

        // Actualiza parcialmente el usuario
        existingUser.first_name = usuarioDTO.first_name ?? existingUser.first_name;
        existingUser.last_name = usuarioDTO.last_name ?? existingUser.last_name;
        existingUser.email = usuarioDTO.email ?? existingUser.email;

        existingUser.modified_at = DateTimeOffset.UtcNow;
        await _iUsuarioDAO.UpdateAsync(existingUser); // Guarda los cambios en la base de datos

        var resultado = _usuarioMapper.ToDTO(existingUser); // Convierte la entidad actualizada a DTO

        try
        {
            var editorUser = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = editorUser?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? editorUser?.Identity?.Name
                          ?? "SISTEMA/AUTO_PATCH",
                Accion = "ACTUALIZACION_PARCIAL_USUARIO",
                Tabla = "Users",
                RegistroId = id.ToString(),
                Detalle = $"Actualización parcial de {infoPrevia}. Datos finales -> Nombre: {existingUser.first_name} {existingUser.last_name}, Email: {existingUser.email}",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al auditar Patch de usuario {id}: {ex.Message}");
        }

        var resultadoDTO = new UsuarioDTOResponceExtends(id, resultado); // Crea un nuevo DTO de respuesta

        return new OkObjectResult(new ApiResponse<UsuarioDTOResponceExtends>(200,
            MessageService.Instance.GetMessage("UpdatePartialAsyncUser200"), resultadoDTO));
    }

    // 7.9.8 Servicio: restaurar un usuario eliminado
    /// <summary>
    /// Restaura un usuario eliminado lógicamente.
    /// </summary>
    /// <param name="id">El identificador único del usuario.</param>
    /// <returns>Un objeto <see cref="UsuarioDTO"/> del usuario restaurado o <c>null</c> si no se encuentra.</returns>
    /// <exception cref="InvalidOperationException">Si el usuario no estaba eliminado.</exception>
    public async Task<IActionResult> RestoreUserAsync(Guid id)
    {
        var restaurarUsuario = await _iUsuarioDAO.GetByIdAsync(id);

        if (restaurarUsuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("RestoreUserAsyncUser404")));
        }

        if (!restaurarUsuario.is_deleted)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("RestoreUserAsyncUser400")));
        }

        restaurarUsuario.is_deleted = false;
        restaurarUsuario.deleted_at = null;
        restaurarUsuario.modified_at = DateTimeOffset.UtcNow;
        await _iUsuarioDAO.UpdateAsync(restaurarUsuario);

        var resultado = _usuarioMapper.ToDTO(restaurarUsuario);
        return new OkObjectResult(new ApiResponse<UsuarioDTO>(200,
            MessageService.Instance.GetMessage("RestoreUserAsyncUser200"), resultado));
    }

    // 7.9.9 Servicio: verificar el email
    /// <summary>
    /// Verifica el correo electrónico de un usuario.
    /// </summary>
    /// <param name="id">El identificador único del usuario.</param>
    /// <returns>El DTO del usuario actualizado, o null si el usuario no existe.</returns>
    /// <exception cref="InvalidOperationException">Se lanza si el correo ya estaba verificado.</exception>
    public async Task<IActionResult> VerifyEmailAsync(Guid id)
    {
        var usuario = await _iUsuarioDAO.GetByIdAsync(id); // Busca el usuario en la base de datos

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("controllerUser404")));
        }

        if (usuario.email_verified)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("VerifyEmailAsyncUser400")));
        }

        usuario.email_verified = true;
        usuario.email_verified_at = DateTime.UtcNow;

        await _iUsuarioDAO.UpdateAsync(usuario);

        var resultado = new UsuarioDTOVerifiedEmail(id, usuario);

        return new OkObjectResult(new ApiResponse<UsuarioDTOVerifiedEmail>(200,
            MessageService.Instance.GetMessage("VerifyEmailAsyncUser200"), resultado));
    }

    // 7.9.10 Servicio: desactivar un usuario
    /// <summary>
    /// Desactiva un usuario estableciendo is_active en false.
    /// </summary>
    /// <param name="id">El identificador único del usuario.</param>
    /// <returns>El DTO del usuario desactivado, o null si no se encuentra.</returns>
    /// <exception cref="InvalidOperationException">Si el usuario ya está desactivado.</exception>
    public async Task<IActionResult> DeactivateUserAsync(Guid id)
    {
        var usuario = await _iUsuarioDAO.GetByIdAsync(id);

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("controllerUser404")));
        }

        if (!usuario.is_active)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("DeactivateUserAsyncUser400")));
        }

        usuario.is_active = false;
        await _iUsuarioDAO.UpdateAsync(usuario);

        var usuarioActivado = _usuarioMapper.ToDTO(usuario);

        var resultado = new UsuarioDTOResponce(id, usuarioActivado);

        return new OkObjectResult(new ApiResponse<UsuarioDTOResponce>(200,
            MessageService.Instance.GetMessage("DeactivateUserAsyncUser200"), resultado));
    }

    // 7.9.11 Servicio: cambiar la contraseña del usuario
    /// <summary>
    /// Cambia la contraseña de un usuario autenticado.
    /// </summary>
    /// <param name="id">Identificador único del usuario (UUID).</param>
    /// <param name="model">DTO con la contraseña actual, nueva contraseña y confirmación.</param>
    /// <returns>Retorna true si la contraseña fue cambiada exitosamente.</returns>
    /// <exception cref="KeyNotFoundException">Se lanza si el usuario no existe.</exception>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si la contraseña actual es incorrecta, la nueva contraseña no cumple los
    /// requisitos o las contraseñas no coinciden.
    /// </exception>
    public async Task<IActionResult> ChangePasswordAsync(Guid id, ChangePasswordDTO model)
    {
        var usuario = await _iUsuarioDAO.GetByIdAsync(id);

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("controllerUser404")));
        }

        // Si la contraseña almacenada NO tiene formato BCrypt, la hasheamos primero
        if (!usuario.password.StartsWith("$2a$") && !usuario.password.StartsWith("$2b$")
            && !usuario.password.StartsWith("$2y$"))
        {
            usuario.password = BCrypt.Net.BCrypt.HashPassword(usuario.password, 12);
            await _iUsuarioDAO.UpdateAsync(usuario);
        }

        // Verificar que la contraseña actual no sea igual a la nueva
        if (model.CurrentPassword == model.NewPassword)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("ChangePasswordAsyncUser400Current")));
        }

        // Verificar que la contraseña actual sea correcta
        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, usuario.password))
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("ChangePasswordAsyncUser400")));
        }

        // Validar que la nueva contraseña cumple con seguridad
        if (model.NewPassword.Length < 8 || !model.NewPassword.Any(char.IsDigit)
            || !model.NewPassword.Any(char.IsLetter))
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("ChangePasswordAsyncUser400Size")));
        }

        // Validar que la confirmación coincida con la nueva contraseña
        if (model.NewPassword != model.ConfirmPassword)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("ChangePasswordAsyncUser400Confirmation")));
        }

        // Hashear la nueva contraseña antes de guardarla
        usuario.password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, 12);
        await _iUsuarioDAO.UpdateAsync(usuario);

        try
        {
            var currentUser = _ihttpContextAccessor.HttpContext?.User;
            var log = new Auditoria
            {
                Usuario = currentUser?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? currentUser?.Identity?.Name
                          ?? "SISTEMA/AUTO_GESTION",
                Accion = "CAMBIO_CONTRASEÑA",
                Tabla = "Users",
                RegistroId = id.ToString(),
                Detalle = $"El usuario {usuario.username} cambió su contraseña exitosamente.",
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al auditar cambio de password para {id}: {ex.Message}");
        }

        return new OkObjectResult(new ApiResponse<string>(200,
            MessageService.Instance.GetMessage("ChangePasswordAsyncUser200")));
    }

    // 7.9.12 Servicio: Proceso inicial de recuperación de cuenta
    // restablecer contraseña
    public async Task<IActionResult> RequestPasswordResetAsync(string email)
    {
        var usuario = await _iUsuarioDAO.GetByEmailAsync(email);

        if (usuario == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("RequestPasswordResetAsyncUser404")));
        }

        if (usuario.email_verified == false)
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("RequestPasswordResetAsyncUser400")));
        }

        // Generar token seguro para restablecimiento (GUID en Base64 sin caracteres conflictivos)
        var tokenBytes = Guid.NewGuid().ToByteArray();
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        usuario.password_reset_token = token;
        var tiempoExpiration = DateTime.UtcNow.AddHours(1); // Expira en 1 hora
        usuario.password_reset_token_expiration = tiempoExpiration;

        await _iUsuarioDAO.UpdateAsync(usuario);

        // se establecen los datos para guardarlos en la base de datos.
        PasswordResetToken resgristoPassword = new PasswordResetToken
        {
            Token = token,
            Email = email,
            Expiration = tiempoExpiration,
            UserId = usuario.id
        };
        await _iUsuarioDAO.addResgistroPassword(resgristoPassword);

        try
        {
            if (usuario?.email == null)
            {
                return new NotFoundObjectResult(new ApiResponse<string>(404,
                    MessageService.Instance.GetMessage("RequestPasswordResetAsyncUser404")));
            }

            await _emailService.SendPasswordResetEmail(usuario.email, token);

            return new OkObjectResult(new ApiResponse<string>(200,
                MessageService.Instance.GetMessage("RequestPasswordResetAsyncUser200")));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error 500: " + ex.Message);
            return new ObjectResult(new ApiResponse<string>(500, "Algo paso en el email"));
        }
    }

    // 7.9.13 Servicio: Continuación del proceso inicial de recuperación de cuenta
    // Recibimos el token y la nueva contraseña
    public async Task<IActionResult> ResetPasswordAsync(string Token, string NewPassword)
    {
        var existeTokenUsuario = await _iUsuarioDAO.GetUsuarioToken(Token);

        if (existeTokenUsuario == null)
        {
            Console.WriteLine("El token expiro");
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("ResetPasswordAsyncUserNull404")));
        }

        var existeToken = await _iUsuarioDAO.GetByToken(Token);

        if (existeToken == null || existeToken.Expiration < DateTime.UtcNow)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("ResetPasswordAsyncUser404")));
        }

        // Verificar que la nueva contraseña cumple con seguridad
        if (NewPassword.Length < 8 || !NewPassword.Any(char.IsDigit) || !NewPassword.Any(char.IsLetter))
        {
            return new BadRequestObjectResult(new ApiResponse<string>(400,
                MessageService.Instance.GetMessage("ResetPasswordAsyncUser400Size")));
        }

        var usuarioActualizar = await _iUsuarioDAO.GetByEmailAsync(existeToken.Email);
        if (usuarioActualizar == null)
        {
            return new NotFoundObjectResult(new ApiResponse<string>(404,
                MessageService.Instance.GetMessage("ResetPasswordAsyncUser404")));
        }

        usuarioActualizar.password = BCrypt.Net.BCrypt.HashPassword(NewPassword, 12);
        usuarioActualizar.password_reset_token = null; // Limpiar el token
        usuarioActualizar.password_reset_token_expiration = null; // Limpiar la expiración del token
        await _iUsuarioDAO.UpdateAsync(usuarioActualizar);

        return new OkObjectResult(new ApiResponse<string>(200,
            MessageService.Instance.GetMessage("ResetPasswordAsyncUser200")));
    }

    /// <summary>
    /// Valida las credenciales de un usuario a partir del correo electrónico y la contraseña proporcionados.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="password">Contraseña en texto plano ingresada por el usuario.</param>
    /// <returns>
    /// Una tarea que representa el resultado de la validación. Retorna un <see cref="UnauthorizedObjectResult"/>
    /// si las credenciales son inválidas o el usuario está inactivo,
    /// o un <see cref="OkObjectResult"/> si las credenciales son válidas.
    /// </returns>
    public async Task<IActionResult> ValidateUserAsync(string email, string password)
    {
        var user = await _iUsuarioDAO.GetByEmailAsync(email);

        if (user == null)
        {
            return new UnauthorizedObjectResult(new ApiResponse<string>(401, "Credenciales inválidas"));
        }

        if (!user.is_active)
        {
            return new UnauthorizedObjectResult(new ApiResponse<string>(401, "Usuario inactivo"));
        }

        // Si la contraseña no está hasheada, la hasheamos y guardamos
        if (!user.password.StartsWith("$2a$") && !user.password.StartsWith("$2b$") && !user.password.StartsWith("$2y$"))
        {
            user.password = BCrypt.Net.BCrypt.HashPassword(user.password, 12);
            await _iUsuarioDAO.UpdateAsync(user);
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.password))
        {
            return new UnauthorizedObjectResult(new ApiResponse<string>(401, "Credenciales inválidas"));
        }

        return new OkObjectResult(user);
    }
}