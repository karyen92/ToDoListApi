using ToDoListApi.Services;

namespace ToDoListApiTest.Base;

public class TestAppSettingProvider : IAppSettingProvider
{
    public string PasswordSalt => "salt";
    public string JwtKey => "123456789101112131415";
    public string JwtIssuer => "issuer";
}