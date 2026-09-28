namespace apitienda.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarLog(string accion, string tabla, string? registroId, string? detalle);
    }
}
