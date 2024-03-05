using FakeItEasy;
using FluentAssertions;
using ToDoListApi.Domains;
using ToDoListApi.Handlers.Tags;
using ToDoListApi.Services;

namespace ToDoListApiTest.Tags;


[TestClass]
public class GetAllTagHandlerSpec : SpecBase
{
    private ToDoListApiDomainContext _domainContext;
    private IIdentityProvider _identityProvider;
    private GetAllTagsHandler _handler;
    private GetAllTagsRequest _request;
    private GetAllTagsResponse _response;

    public override void Before()
    {
        _domainContext = Resolve<ToDoListApiDomainContext>();
        _domainContext.Database.EnsureCreated();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            Password = "abc",
            CreateDate = DateTime.UtcNow
        };
        
        _domainContext.Users.Add(user);
        
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Label = "Test",
            CreatedByUserId = user.Id,
            LastUpdateDate = DateTime.UtcNow
        };

        _domainContext.Tags.Add(tag);

        _domainContext.SaveChanges();
        
        _identityProvider = A.Fake<IIdentityProvider>();
        A.CallTo(() => _identityProvider.UserId).Returns(user.Id);

        _handler = new GetAllTagsHandler(_domainContext, _identityProvider);
    }

    public override void When()
    {
        _request = new GetAllTagsRequest();

        _response = _handler.Handle(_request, CancellationToken.None).Result;
    }

    [TestMethod]
    public void Ensure_Create_Success()
    {
        _response.Data.Count.Should().Be(1);
        
        var data = _response.Data[0];
        var dbObject = _domainContext.Tags.Single(x => x.Id == _response.Data[0].Id);

        data.Label.Should().Be(dbObject.Label);
        data.LastUpdateDate.Should().Be(dbObject.LastUpdateDate);
    }
}
