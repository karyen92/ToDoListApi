using ToDoListApi.Services;

namespace ToDoListApiTest.Base;

public class TestAppSettingProvider : IAppSettingProvider
{
    public string PasswordSalt => "salt";
    public string JwtKey => "1234567891011121314152243421443534543";
    public string JwtIssuer => "issuer";
}