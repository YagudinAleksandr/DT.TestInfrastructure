using Npgsql;

namespace DT.TestInfrastructure
{
    /// <summary>
    /// Клинер всех таблиц БД
    /// </summary>
    public static class DatabaseCleaner
    {
        /// <summary>
        /// Очистка всех таблиц
        /// </summary>
        /// <param name="connectionString">Строка подклбчения</param>
        public static async Task CleanAllTablesAsync(string connectionString)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            // Отключаем внешние ключи
            await using (var cmd = new NpgsqlCommand("SET session_replication_role = 'replica';", conn))
                await cmd.ExecuteNonQueryAsync();

            // Получаем все таблицы
            var tables = new List<string>();
            await using (var cmd = new NpgsqlCommand("""
            SELECT tablename FROM pg_tables 
            WHERE schemaname = 'public'
            """, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    tables.Add(reader.GetString(0));
            }

            // Удаляем данные
            if (tables.Count > 0)
            {
                var truncateSql = "TRUNCATE TABLE " + string.Join(", ", tables.Select(t => $"\"{t}\"")) + " RESTART IDENTITY CASCADE;";
                await using var truncateCmd = new NpgsqlCommand(truncateSql, conn);
                await truncateCmd.ExecuteNonQueryAsync();
            }

            // Включаем внешние ключи обратно
            await using (var cmd = new NpgsqlCommand("SET session_replication_role = 'origin';", conn))
                await cmd.ExecuteNonQueryAsync();
        }
    }
}
