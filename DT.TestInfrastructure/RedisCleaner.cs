using StackExchange.Redis;

namespace DT.TestInfrastructure
{
    /// <summary>
    /// Очистка данных Redis.
    /// </summary>
    public static class RedisCleaner
    {
        /// <summary>
        /// Очищает текущую базу данных Redis (аналог TRUNCATE для PostgreSQL).
        /// </summary>
        /// <param name="connectionString">Строка подключения к Redis.</param>
        public static async Task CleanAsync(string connectionString)
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            var db = redis.GetDatabase();

            // Очищаем только текущую БД (обычно DB 0)
            await db.ExecuteAsync("FLUSHDB");

            // Не вызываем Dispose — Multiplexer может переиспользоваться
            // Но в тестах — можно и игнорировать
        }
    }
}
