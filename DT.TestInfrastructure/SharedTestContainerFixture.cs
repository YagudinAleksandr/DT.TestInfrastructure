using Testcontainers.PostgreSql;

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
        /// Строка подклбчения к базе данных
        /// </summary>
        public string ConnectionString => _container?.GetConnectionString()
            ?? throw new InvalidOperationException("Container not initialized.");

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

            await _container.StartAsync();
        }

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            if (_container is not null)
                await _container.DisposeAsync();
        }
    }
}