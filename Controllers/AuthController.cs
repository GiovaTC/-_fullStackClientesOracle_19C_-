using Backend.Data;
using Backend.Dtos;
using Backend.Security;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(
    OracleConnectionFactory factory, JwtTokenService tokens) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            await using var cn = factory.Create();
            await cn.OpenAsync();

            // Se obtiene el hash almacenado para validar PBKDF2 en la aplicación.
            // El paquete PL/SQL valida la identidad/estado y la coincidencia del hash.
            string? storedHash = null;
            await using (var find = new OracleCommand(
                "SELECT password_hash FROM usuarios WHERE UPPER(nombre_usuario)=UPPER(:u) AND activo='S'", cn))
            {
                find.Parameters.Add("u", OracleDbType.Varchar2, request.Usuario, ParameterDirection.Input);
                var value = await find.ExecuteScalarAsync();
                storedHash = value == null || value == DBNull.Value ? null : value.ToString();
            }

            if (storedHash == null || !PasswordHasher.Verify(request.Password, storedHash))
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

            await using var cmd = new OracleCommand("pkg_seguridad.validar_usuario", cn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("p_nombre_usuario", OracleDbType.Varchar2, request.Usuario, ParameterDirection.Input);
            cmd.Parameters.Add("p_password_hash", OracleDbType.Varchar2, storedHash, ParameterDirection.Input);
            cmd.Parameters.Add("p_id_usuario", OracleDbType.Int32).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_nombre", OracleDbType.Varchar2, 60).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_estado", OracleDbType.Int32).Direction = ParameterDirection.Output;
            cmd.Parameters.Add("p_mensaje", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
            await cmd.ExecuteNonQueryAsync();

            if (Convert.ToInt32(cmd.Parameters["p_estado"].Value.ToString()) != 1)
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

            int id = Convert.ToInt32(cmd.Parameters["p_id_usuario"].Value.ToString());
            string nombre = cmd.Parameters["p_nombre"].Value.ToString()!;
            return Ok(tokens.Create(id, nombre));
        }
    }   
}
