using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apitienda.Models
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public string Accion { get; set; } = string.Empty;

        [Required]
        public string Tabla { get; set; } = string.Empty;

        public string? RegistroId { get; set; }

        public string? Detalle { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string? IpAddress { get; set; }
    }
}
