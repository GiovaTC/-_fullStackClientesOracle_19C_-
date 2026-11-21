using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public sealed class ClienteRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Nombres { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 2)]
        public string Apellidos { get; set; } = "";

        [Required, EmailAddress, StringLength(150)]
        public string Correo { get; set; } = "";

        [StringLength(30)]
        public string? Telefono { get; set; }
    }

    public sealed record ClienteResponse(
    int IdCliente, string Nombres, string Apellidos,
    string Correo, string? Telefono, DateTime FechaRegistro);
}
