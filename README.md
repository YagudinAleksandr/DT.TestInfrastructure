# DT.TestInfrastructure

## Описание

Библиотека с Testcontainers для интеграционных тестов.

## Пример использования

В проект с тестами внедрить зависимость:

```xml
<ProjectReference Include="..\..\..\DT.TestInfrastructure\DT.TestInfrastructure\DT.TestInfrastructure.csproj" />
```

В проекте тестов создать Fixture и коллекцию тестов

Определение коллекции:
```csharp
[CollectionDefinition(nameof(OrgUsersServiceCollection))]
public class OrgUsersServiceCollection : ICollectionFixture<SharedTestContainerFixture>;
```

Создать базовый клас, для провайдера
```csharp
[Collection(nameof(OrgUsersServiceCollection))]
    public abstract class OrgUsersServiceTestsBase : IAsyncLifetime
    {
        private readonly string ConnectionString;
        
        public IServiceProvider ServiceProvider { get; }

        protected OrgUsersServiceTestsBase(SharedTestContainerFixture fixture)
        {
            ConnectionString = fixture.ConnectionString;
            ServiceProvider = TestServiceProvider.CreateServiceProvider(ConnectionString);
        }

        public virtual async Task InitializeAsync()
        {
            await ApplyMigration();
        }
        public virtual async Task DisposeAsync()
        {
            await DatabaseCleaner.CleanAllTablesAsync(ConnectionString);
        }

        private async Task ApplyMigration()
        {
            var context = ServiceProvider.GetRequiredService<MainContext>();
            await context.Database.MigrateAsync();
        }
    }
```

Интеграционные тесты наследовать от базового класса
```csharp
 public class UserRepositoryTests : OrgUsersServiceTestsBase
    {
        public UserRepositoryTests(SharedTestContainerFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task AddUser_ShouldPersist_WhenCommitted()
        {
            // Arrange
            var unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            var repository = ServiceProvider.GetRequiredService<IUserRepository>();

            var user = User.Create(new Username("Test user"), new Email("test@example.com"), "hashPassword", "saltPassword", UserStatus.Pending, UserType.Admin);

            // Act
            repository.Add(user);
            await unitOfWork.CommitAsync();

            // Assert
            var found = await repository.GetByEmail("test@example.com");
            Assert.NotNull(found);
            Assert.Equal("test@example.com", found!.Email.Value);
        }
    }
```

## Redis

Как использовать в тестах:

```csharp
using DT.TestInfrastructure;
using Xunit;

[CollectionDefinition("SharedContainer")]
public class SharedContainerCollection : ICollectionFixture<SharedTestContainerFixture> { }

[Collection("SharedContainer")]
public class MyIntegrationTests
{
    private readonly SharedTestContainerFixture _fixture;

    public MyIntegrationTests(SharedTestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Очищаем PostgreSQL
        await DatabaseCleaner.CleanAllTablesAsync(_fixture.ConnectionString);
        // Очищаем Redis
        await RedisCleaner.CleanAsync(_fixture.ConnectionRedis);
    }

    [Fact]
    public async Task Test_Something()
    {
        await InitializeAsync(); // или вызывать в каждом тесте / через IAsyncLifetime в тестовом классе

        // Ваш тест...
    }
}
```

Пример:

```csharp
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly SharedTestContainerFixture Fixture;

    protected IntegrationTestBase(SharedTestContainerFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await DatabaseCleaner.CleanAllTablesAsync(Fixture.ConnectionString);
        await RedisCleaner.CleanAsync(Fixture.ConnectionRedis);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```

Тест:

```csharp
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly SharedTestContainerFixture Fixture;

    protected IntegrationTestBase(SharedTestContainerFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await DatabaseCleaner.CleanAllTablesAsync(Fixture.ConnectionString);
        await RedisCleaner.CleanAsync(Fixture.ConnectionRedis);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
```