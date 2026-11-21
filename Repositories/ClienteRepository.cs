using System.Data;
using Backend.Data;
using Backend.Dtos;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Repositories
{
    public sealed class ClienteRepository(OracleConnectionFactory factory)
    {
        private static OracleParameter In(string name, OracleDbType type, object? value, int size = 0)
        {
            var p = new OracleParameter(name, type) { Direction = ParameterDirection.Input, Value = value ?? DBNull.Value };
            if (size > 0) p.Size = size;
            return p;
        }

        private static OracleParameter Out(string name, OracleDbType type, int size = 0)
        {
            var p = new OracleParameter(name, type) { Direction = ParameterDirection.Output };
            if (size > 0) p.Size = size;
            return p;
        }

        private static ClienteResponse Map(OracleDataReader r) => new(
            Convert.ToInt32(r["ID_CLIENTE"]),
            r["NOMBRES"].ToString()!,
            r["APELLIDOS"].ToString()!,
            r["CORREO"].ToString()!,
            r["TELEFONO"] == DBNull.Value ? null : r["TELEFONO"].ToString(),
            Convert.ToDateTime(r["FECHA_REGISTRO"]));
        
        public async Task<List<ClienteResponse>> Listar()
        {
            await using var cn = factory.Create();
            await cn.OpenAsync();
            await using var cmd = new OracleCommand("pkg_clientes.listar", cn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("p_resultado", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<ClienteResponse>();
            while (await r.ReadAsync()) list.Add(Map(r));
            return list;
        }   
    }
}
