using Backend.Dtos;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/clientes")]
    public sealed class ClientesController(ClienteRepository repo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try { return Ok(await repo.Listar()); }
            catch (OracleException) { return Problem("Error consultando Oracle.", statusCode: 500); }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            if (id <= 0) return BadRequest(new { mensaje = "Identificador inválido." });
            try
            {
                var cliente = await repo.Obtener(id);
                return cliente is null ? NotFound() : Ok(cliente);
            }
            catch (OracleException) { return Problem("Error consultando Oracle.", statusCode: 500); }
        }

        [HttpPost]
        public async Task<IActionResult> Crear(ClienteRequest request)
        {
            try
            {
                int id = await repo.Insertar(request);
                var creado = await repo.Obtener(id);
                return CreatedAtAction(nameof(Obtener), new { id }, creado);
            }
            catch (OracleException ex) when (ex.Number == 1)
            { return Conflict(new { mensaje = "Ya existe un cliente con ese correo." }); }
            catch (OracleException) { return Problem("No fue posible registrar el cliente.", statusCode: 500); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, ClienteRequest request)
        {
            if (id <= 0) return BadRequest();
            try
            {
                bool actualizado = await repo.Actualizar(id, request);
                return actualizado ? NoContent() : NotFound();
            }
            catch (OracleException ex) when (ex.Number == 1)
            { return Conflict(new { mensaje = "Ya existe un cliente con ese correo." }); }
            catch (OracleException) { return Problem("No fue posible actualizar el cliente.", statusCode: 500); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id <= 0) return BadRequest();
            try { return await repo.Eliminar(id) ? NoContent() : NotFound(); }
            catch (OracleException) { return Problem("No fue posible eliminar el cliente.", statusCode: 500); }
        }
    }
}
