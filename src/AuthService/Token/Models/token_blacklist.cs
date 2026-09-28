using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace apitienda.Models
{
    /// <summary>
    /// Representa un refresh token revocado para invalidarlo antes de su expiración.
    /// </summary>
    public class token_blacklist
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identificador único del JWT (claim "jti").
        /// </summary>
        [Required]
        public string jti { get; set; } = null!;

        /// <summary>
        /// Identificador del usuario propietario del token.
        /// </summary>
        [Required]
        public Guid user_id { get; set; }

        /// <summary>
        /// Fecha y hora en que se revocó el token.
        /// </summary>
        public DateTimeOffset? revoked_at { get; set; }

        /// <summary>
        /// Fecha y hora en que expiraba el token original.
        /// </summary>
        [Required]
        public DateTimeOffset expires_at { get; set; }

        /// <summary>
        /// Motivo de la revocación (logout, refresh, seguridad, etc.).
        /// </summary>
        public string? reason { get; set; }
    }
}
