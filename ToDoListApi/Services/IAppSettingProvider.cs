namespace ToDoListApi.Services;

public interface IAppSettingProvider
{
    string PasswordSalt { get; }
    string JwtKey { get; }
    string JwtIssuer { get; }
}

public class AppSettingProvider : IAppSettingProvider
{
    public AppSettingProvider(IConfiguration configuration)
    {
        PasswordSalt = configuration["App:PasswordSalt"];
        JwtKey = configuration["Jwt:Key"];
        JwtIssuer = configuration["Jwt:Issuer"];
    }

    public string PasswordSalt { get; }
    public string JwtKey { get; }
    public string JwtIssuer { get; }
}