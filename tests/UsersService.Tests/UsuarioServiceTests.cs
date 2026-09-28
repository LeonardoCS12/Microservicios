using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using apitienda.Models;
using apitienda.Services;
using apitienda.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiTienda.Tests
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioDAO> _usuarioDaoMock;
        private readonly Mock<PasswordResetEmail> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextMock;
        private readonly UsuarioMapper _usuarioMapper;
        private readonly DataContext _context; // Se usa una DB en memoria real
        private readonly UsuarioService _usuarioService;

        public UsuarioServiceTests()

        {

            _usuarioDaoMock = new Mock<IUsuarioDAO>();
            _emailServiceMock = new Mock<PasswordResetEmail>();
            _httpContextMock = new Mock<IHttpContextAccessor>();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            _context = new DataContext(options);



            // 3. Simulación del usuario en el HttpContext (Para la Auditoría)
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "TestUserAdmin") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextMock.Setup(h => h.HttpContext).Returns(httpContext);

            // 4. Mappers
            var usuarioMapper = new UsuarioMapper();
            var createUserMapper = new CreateUserMapper();

            // 5. Inicialización del Service con el orden exacto del constructor
            _usuarioService = new UsuarioService(
                _httpContextMock.Object,
                _context,
                _usuarioDaoMock.Object,
                usuarioMapper,
                createUserMapper,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllAsync2_RetornaOk_CuandoExistenUsuarios()
        {
            // Arrange
            var listaUsuarios = new List<Usuario>
            {
                new Usuario
                {
                    id = Guid.NewGuid(),
                    email = "test1@example.com",
                    username = "testuser1",
                    first_name = "Nombre1",
                    last_name = "Apellido1",
                    is_active = true,
                    password = "hashed_password_requerida"
                }
            };

            // Simula que el DAO devuelve la lista de usuarios
            _usuarioDaoMock.Setup(d => d.GetUserAsync(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .ReturnsAsync(listaUsuarios);

            // Act
            // Llama al servicio con los parámetros nulos (filtros)
            var result = await _usuarioService.GetAllAsync2(null, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verificación robusta usando JSON para confirmar que los datos llegaron
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("test1@example.com", jsonResponse);
        }
        [Fact]
        public async Task ActivateUserAsync_RetornaOk_CuandoActivacionEsExitosa()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioInactivo = new Usuario
            {
                id = userId,
                is_active = false,
                email = "test@test.com",
                username = "testuser",
                first_name = "Elizabeth",
                last_name = "Dev",
                password = "123"
            };

            // Simula que el DAO encuentra al usuario
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId))
                           .ReturnsAsync(usuarioInactivo);

            var result = await _usuarioService.ActiveteUserAsync(userId);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verifica que el objeto en la base de datos (el mock) se activó
            Assert.True(usuarioInactivo.is_active);

            // En lugar de buscar "is_active":true en el JSON, solo verifica que diga "usuario activado"
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("usuario activado", jsonResponse.ToLower());

            // Verifica que se llamó al método Update en el DAO
            _usuarioDaoMock.Verify(d => d.UpdateAsync(usuarioInactivo), Times.Once);
        }

        [Fact]
        public async Task ActivateUserAsync_RetornaBadRequest_CuandoUsuarioYaEstaActivo()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioActivo = new Usuario { id = userId, is_active = true, password = "123" };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioActivo);

            // Act
            var result = await _usuarioService.ActiveteUserAsync(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            _usuarioDaoMock.Verify(d => d.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_RetornaOk_CuandoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuario = new Usuario { id = userId, email = "elizabeth@test.com", username = "elizabeth", password = "hashed_password" };
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuario);

            // Act
            var result = await _usuarioService.GetByIdAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }


        [Fact]
        public async Task GetByIdAsync_RetornaNotFound_CuandoUsuarioNoExiste()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Simula que el DAO devuelve null (el usuario no existe en la base de datos)
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId))
                           .ReturnsAsync((Usuario)null!);

            // Act
            var result = await _usuarioService.GetByIdAsync(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);


        }
        [Fact]
        public async Task AddAsync_RetornaCreated_Y_RegistraAuditoria()
        {
            // Arrange
            var dto = new UsuarioCreateDTO
            {
                email = "nuevo@test.com",
                username = "nuevoUser",
                password = "Password123",
                first_name = "Elizabeth",
                last_name = "Dev",

                is_active = true,
            };

            // Simular que no existe el usuario (lista vacía)
            _usuarioDaoMock.Setup(d => d.GetUserAsync(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .ReturnsAsync(new List<Usuario>());

            // Act
            var result = await _usuarioService.AddAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            // Verificar que se guardó algo en la tabla de Auditorías de la DB en memoria
            Assert.True(_context.Auditorias.Any(a => a.Accion == "CREAR_USUARIO"));
            _usuarioDaoMock.Verify(d => d.AddAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_RetornaConflict_CuandoEmailOUsernameYaExisten()
        {
            // Arrange
            var dto = new UsuarioCreateDTO
            {
                email = "existe@test.com",
                username = "existe",
                password = "123",       // REQUERIDO
                first_name = "N",       // REQUERIDO
                last_name = "A",        // REQUERIDO
                is_active = true        // REQUERIDO
            };

            var listaExistente = new List<Usuario> { new Usuario { email = dto.email, password = "123" } };

            _usuarioDaoMock.Setup(d => d.GetUserAsync(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .ReturnsAsync(listaExistente);

            // Act
            var result = await _usuarioService.AddAsync(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, objectResult.StatusCode);
            _usuarioDaoMock.Verify(d => d.AddAsync(It.IsAny<Usuario>()), Times.Never);
        }


        [Fact]
        public async Task UpdateAsync_RetornaOk_CuandoActualizacionEsExitosa()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioUpdate = new UsuarioUpdate
            {
                email = "cambio@test.com",
                username = "nuevoUser",
                password = "newpassword",
                first_name = "Editado",
                last_name = "User",
                is_active = true,
                is_superuser = false,
                role = "User",
                profile_picture = "foto.png",
                nacionality = "Mexicana",
                occupation = "Developer",
                contact_phone_number = "123456789",
                gender = "F",
                address = "Calle Conocida",
                address_number = "123",
                address_interior_number = "N/A",
                address_complement = "Cerca de la tienda",
                address_neighborhood = "Centro",
                address_zip_code = "70800",
                address_city = "Tlaxiaco",
                address_state = "Oaxaca",

                date_of_birth = DateTime.Now
            };

            var usuarioExistente = new Usuario
            {
                id = userId,
                email = "viejo@test.com",
                username = "viejoUser",
                is_active = true,
                is_deleted = false,
                password = "oldpassword"
            };

            // 1. Simula que el usuario a editar existe
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioExistente);

            // 2. Simula que el nuevo email/username no está tomado por nadie más (lista vacía)
            _usuarioDaoMock.Setup(d => d.GetUserAsync(It.IsAny<string>(), It.IsAny<Dapper.DynamicParameters>()))
                           .ReturnsAsync(new List<Usuario>());

            // Act
            var result = await _usuarioService.UpdateAsync(userId, usuarioUpdate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verificación robusta del contenido
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("cambio@test.com", jsonResponse);
            Assert.Contains("nuevoUser", jsonResponse);

            // Verifica que el DAO recibió la orden de actualizar
            _usuarioDaoMock.Verify(d => d.UpdateAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RetornaOk_CuandoEliminacionEsExitosa()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioExistente = new Usuario
            {
                id = userId,
                is_deleted = false,
                password = "123"
            };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioExistente);
            _usuarioDaoMock.Setup(d => d.DeleteAsync(userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _usuarioService.DeleteAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var response = Assert.IsAssignableFrom<ApiResponse<string>>(okResult.Value);
            Assert.Equal(200, response.Codigo);

            _usuarioDaoMock.Verify(d => d.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RetornaBadRequest_CuandoUsuarioYaEstaEliminado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioYaEliminado = new Usuario
            {
                id = userId,
                is_deleted = true,
                password = "123"
            };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioYaEliminado);

            // Act
            var result = await _usuarioService.DeleteAsync(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);

            _usuarioDaoMock.Verify(d => d.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePartialAsync_RetornaOk_CuandoActualizacionParcialEsExitosa()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new Usuario
            {
                id = userId,
                first_name = "Original",
                last_name = "User",
                email = "original@test.com",
                username = "originalUser",
                is_deleted = false,
                is_active = true,
                password = "123"
            };

            var patchDto = new UsuarioPatchDTO
            {
                first_name = "NuevoNombre"
            };

            // 1. El DAO encuentra al usuario original
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(existingUser);

            // Act
            var result = await _usuarioService.UpdatePartialAsync(userId, patchDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verifica que el objeto real se actualizó internamente
            Assert.Equal("NuevoNombre", existingUser.first_name);

            // Verifica que los datos se enviaron al JSON de respuesta correctamente
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("NuevoNombre", jsonResponse);

            // Verifica que se llamó al Update del DAO
            _usuarioDaoMock.Verify(d => d.UpdateAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task RestoreUserAsync_RetornaOk_CuandoUsuarioEsRestaurado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioEliminado = new Usuario
            {
                id = userId,
                is_deleted = true,
                deleted_at = DateTimeOffset.UtcNow,
                email = "test@test.com",
                username = "user1",
                first_name = "Elizabeth", // Datos para el mapper real
                last_name = "Ferretería",
                password = "123"
            };

            // Simula que el DAO encuentra al usuario eliminado
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId))
                           .ReturnsAsync(usuarioEliminado);
            // Act
            var result = await _usuarioService.RestoreUserAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // Verifica que las banderas de eliminación se resetearon
            Assert.False(usuarioEliminado.is_deleted);
            Assert.Null(usuarioEliminado.deleted_at);

            // Verifica la respuesta con JSON para evitar errores de nombres de propiedades
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("\"is_deleted\":false", jsonResponse.ToLower());

            // Verifica que se llamó al Update del DAO para guardar los cambios
            _usuarioDaoMock.Verify(d => d.UpdateAsync(usuarioEliminado), Times.Once);
        }
        [Fact]
        public async Task RestoreUserAsync_RetornaBadRequest_CuandoUsuarioNoEstabaEliminado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioActivo = new Usuario { id = userId, is_deleted = false, password = "123" };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioActivo);

            // Act
            var result = await _usuarioService.RestoreUserAsync(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
            _usuarioDaoMock.Verify(d => d.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task VerifyEmailAsync_RetornaOk_CuandoVerificacionEsExitosa()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioNoVerificado = new Usuario
            {
                id = userId,
                email = "test@test.com",
                email_verified = false,
                password = "123"
            };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioNoVerificado);
            _usuarioDaoMock.Setup(d => d.UpdateAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);

            // Act
            var result = await _usuarioService.VerifyEmailAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse<UsuarioDTOVerifiedEmail>>(okResult.Value);

            Assert.Equal(200, okResult.StatusCode);
            Assert.True(usuarioNoVerificado.email_verified);
            Assert.NotNull(usuarioNoVerificado.email_verified_at);

            _usuarioDaoMock.Verify(d => d.UpdateAsync(usuarioNoVerificado), Times.Once);
        }

        [Fact]
        public async Task VerifyEmailAsync_RetornaBadRequest_CuandoYaEstabaVerificado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioVerificado = new Usuario
            {
                id = userId,
                email_verified = true,
                password = "123"
            };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuarioVerificado);

            // Act
            var result = await _usuarioService.VerifyEmailAsync(userId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);

            // Verifica que no se intentó actualizar nada
            _usuarioDaoMock.Verify(d => d.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        #region DeactivateUserAsync Tests
        [Fact]
        public async Task DeactivateUserAsync_RetornaOk_CuandoUsuarioEsDesactivado()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuarioActivo = new Usuario
            {
                id = userId,
                is_active = true,
                email = "test@test.com",
                username = "user1",
                first_name = "Elizabeth",
                last_name = "Dev",
                password = "123"
            };

            // Simula que el DAO encuentra al usuario activo
            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId))
                           .ReturnsAsync(usuarioActivo);

            // Act
            var result = await _usuarioService.DeactivateUserAsync(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            // LA PRUEBA CLAVE: Verifica que el objeto en memoria realmente se desactivó
            Assert.False(usuarioActivo.is_active);

            // Verifica que la respuesta sea exitosa buscando el código 200 en el JSON
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("200", jsonResponse);

            // Verifica que se llamó al método Update en el DAO
            _usuarioDaoMock.Verify(d => d.UpdateAsync(usuarioActivo), Times.Once);
        }
        #endregion
        #region ChangePasswordAsync Tests
        [Fact]
        public async Task ChangePasswordAsync_RetornaOk_CuandoContrasenaEsValida()
        {
            // Arrange
            var userId = Guid.NewGuid();
            string passwordActual = "OldPass123";
            string passwordNueva = "NewPass123";

            var usuario = new Usuario
            {
                id = userId,
                password = BCrypt.Net.BCrypt.HashPassword(passwordActual, 12)
            };

            var model = new ChangePasswordDTO
            {
                CurrentPassword = passwordActual,
                NewPassword = passwordNueva,
                ConfirmPassword = passwordNueva
            };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuario);

            // Act
            var result = await _usuarioService.ChangePasswordAsync(userId, model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            Assert.True(BCrypt.Net.BCrypt.Verify(passwordNueva, usuario.password));
            _usuarioDaoMock.Verify(d => d.UpdateAsync(usuario), Times.AtLeastOnce());
        }

        [Fact]
        public async Task ChangePasswordAsync_RetornaBadRequest_CuandoConfirmacionNoCoincide()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var usuario = new Usuario { id = userId, password = BCrypt.Net.BCrypt.HashPassword("Current123") };
            var model = new ChangePasswordDTO
            {
                CurrentPassword = "Current123",
                NewPassword = "NewPassword123",
                ConfirmPassword = "DIFERENTE"
            };

            _usuarioDaoMock.Setup(d => d.GetByIdAsync(userId)).ReturnsAsync(usuario);

            // Act
            var result = await _usuarioService.ChangePasswordAsync(userId, model);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }
        #endregion

        [Fact]
        public async Task ValidateUserAsync_RetornaOk_CuandoCredencialesSonCorrectas()
        {
            // Arrange
            string email = "test@test.com";
            string passwordClaro = "Password123";
            var usuario = new Usuario
            {
                email = email,
                password = BCrypt.Net.BCrypt.HashPassword(passwordClaro, 12),
                is_active = true
            };

            _usuarioDaoMock.Setup(d => d.GetByEmailAsync(email)).ReturnsAsync(usuario);

            // Act
            var result = await _usuarioService.ValidateUserAsync(email, passwordClaro);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var userResponse = Assert.IsType<Usuario>(okResult.Value);
            Assert.Equal(email, userResponse.email);
        }

        [Fact]
        public async Task ValidateUserAsync_RetornaUnauthorized_CuandoUsuarioEstaInactivo()
        {
            // Arrange
            string email = "inactivo@test.com";
            var usuarioInactivo = new Usuario { email = email, is_active = false, password = "any" };

            _usuarioDaoMock.Setup(d => d.GetByEmailAsync(email)).ReturnsAsync(usuarioInactivo);

            // Act
            var result = await _usuarioService.ValidateUserAsync(email, "password");

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsAssignableFrom<ApiResponse<string>>(unauthorized.Value);
            Assert.Equal("Usuario inactivo", response.Mensaje);
        }

        [Fact]
        public async Task ValidateUserAsync_MigraPasswordABcrypt_CuandoEstaEnTextoPlano()
        {
            // Arrange
            string email = "migracion@test.com";
            string passTextoPlano = "12345";
            var usuarioOld = new Usuario { email = email, password = passTextoPlano, is_active = true };

            _usuarioDaoMock.Setup(d => d.GetByEmailAsync(email)).ReturnsAsync(usuarioOld);

            // Act
            await _usuarioService.ValidateUserAsync(email, passTextoPlano);

            // Assert
            // Verifica que se llamó al Update para guardar el nuevo Hash
            _usuarioDaoMock.Verify(d => d.UpdateAsync(It.Is<Usuario>(u => u.password.StartsWith("$2"))), Times.Once);
        }

    }
}
