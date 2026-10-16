namespace RaceDay.API.Services;

public interface IRaceDayPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
