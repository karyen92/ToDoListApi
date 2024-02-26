using FluentValidation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDoListApi.Domains;
using ToDoListApi.Services;

namespace ToDoListApiTest.Base;

public abstract class SpecBase
{
    private SqliteConnection _connection;
    private IServiceScope _serviceScope;
    private IServiceProvider _services;

    [TestInitialize]
    public void Init()
    {

        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDbContext<ToDoListApiDomainContext>(o =>
        {
            o.UseSqlite(_connection);

        });
        serviceCollection.AddSingleton<IAppSettingProvider, TestAppSettingProvider>();
        serviceCollection.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<User>());
        serviceCollection.AddValidatorsFromAssemblyContaining(typeof(User));
        _services = serviceCollection.BuildServiceProvider();
        _serviceScope = _services.CreateScope();
        
        Before();
        When();
    }
    
    public virtual void Before() { }
    public virtual void When() { }
    public virtual void After() { }
    
    [TestCleanup]
    public void TearDown()
    {
        After();
        _connection.Close();
    }

    protected TService Resolve<TService>()
    {
        return _serviceScope.ServiceProvider.GetService<TService>() ?? throw new InvalidOperationException();
    }
}