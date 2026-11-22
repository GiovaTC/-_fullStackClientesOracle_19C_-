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
        
        public async Task<ClienteResponse?> Obtener(int id)
        {
            await using var cn = factory.Create();
            await cn.OpenAsync();
            await using var cmd = new OracleCommand("pkg_clientes.obtener", cn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(In("p_id", OracleDbType.Int32, id));
            cmd.Parameters.Add("p_resultado", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            await using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync() ? Map(r) : null;
        }
        
        public async Task<int> Insertar(ClienteRequest x)
        {
            await using var cn = factory.Create();
            await cn.OpenAsync();
            await using var cmd = new OracleCommand("pkg_clientes.insertar", cn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(In("p_nombres", OracleDbType.Varchar2, x.Nombres, 100));
            cmd.Parameters.Add(In("p_apellidos", OracleDbType.Varchar2, x.Apellidos, 100));
            cmd.Parameters.Add(In("p_correo", OracleDbType.Varchar2, x.Correo, 150));
            cmd.Parameters.Add(In("p_telefono", OracleDbType.Varchar2, x.Telefono, 30));
            cmd.Parameters.Add(Out("p_id", OracleDbType.Int32));
            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(cmd.Parameters["p_id"].Value.ToString());
        }

        public async Task<bool> Actualizar(int id, ClienteRequest x)
        {
            await using var cn = factory.Create();
            await cn.OpenAsync();
            await using var cmd = new OracleCommand("pkg_clientes.actualizar", cn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(In("p_id", OracleDbType.Int32, id));
            cmd.Parameters.Add(In("p_nombres", OracleDbType.Varchar2, x.Nombres, 100));
            cmd.Parameters.Add(In("p_apellidos", OracleDbType.Varchar2, x.Apellidos, 100));
            cmd.Parameters.Add(In("p_correo", OracleDbType.Varchar2, x.Correo, 150));
            cmd.Parameters.Add(In("p_telefono", OracleDbType.Varchar2, x.Telefono, 30));
            cmd.Parameters.Add(Out("p_filas", OracleDbType.Int32));
            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(cmd.Parameters["p_filas"].Value.ToString()) > 0;
        }

        public async Task<bool> Eliminar(int id)
        {
            await using var cn = factory.Create();
            await cn.OpenAsync();
            await using var cmd = new OracleCommand("pkg_clientes.eliminar", cn)
            { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(In("p_id", OracleDbType.Int32, id));
            cmd.Parameters.Add(Out("p_filas", OracleDbType.Int32));
            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(cmd.Parameters["p_filas"].Value.ToString()) > 0;
        }
    }   
}
