using apitienda.Data;
using apitienda.Models;

namespace apitienda.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarLog(string accion, string tabla, string? registroId, string? detalle)
        {
            var usuario = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema/Anónimo";

            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var log = new Auditoria
            {
                Usuario = usuario,
                Accion = accion,
                Tabla = tabla,
                RegistroId = registroId,
                Detalle = detalle,
                IpAddress = ip,
                Fecha = DateTime.UtcNow
            };

            _context.Auditorias.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
