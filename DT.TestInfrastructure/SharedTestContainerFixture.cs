using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace DT.TestInfrastructure
{
    /// <summary>
    /// Общая фикстура для проектов
    /// </summary>
    public class SharedTestContainerFixture : IAsyncLifetime
    {
        /// <summary>
        /// Контейнер PostgreSQL
        /// </summary>
        private PostgreSqlContainer? _container;

        /// <summary>
        /// Контейнер Redis
        /// </summary>
        private RedisContainer? _redisContainer;

        /// <summary>
        /// Строка подклбчения к базе данных
        /// </summary>
        public string ConnectionString => _container?.GetConnectionString()
            ?? throw new InvalidOperationException("Container not initialized.");

        /// <summary>
        /// Строка подключения к Redis
        /// </summary>
        public string ConnectionRedis => _redisContainer?.GetConnectionString()
            ?? throw new InvalidOperationException("Container Redis not initialized");

        /// <summary>
        /// Инициализация
        /// </summary>
        public async Task InitializeAsync()
        {
            _container = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCleanUp(true)
                .Build();

            // Запуск Redis
            _redisContainer = new RedisBuilder()
                .WithCleanUp(true)
                .Build();

            // Запускаем оба параллельно для ускорения
            await Task.WhenAll(
                _container.StartAsync(),
                _redisContainer.StartAsync()
            );
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            var tasks = new List<Task>();

            if (_container != null)
                tasks.Add(_container.DisposeAsync().AsTask());

            if (_redisContainer != null)
                tasks.Add(_redisContainer.DisposeAsync().AsTask());

            if (tasks.Count > 0)
                await Task.WhenAll(tasks);
        }
    }
}