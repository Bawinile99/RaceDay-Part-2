namespace RaceDay.API.Services;

public static class SessionExtensions
{
    private const string UserIdKey = "UserId";
    private const string RoleKey = "Role";

    public static void SetUser(this ISession session, int userId, string role)
    {
        session.SetInt32(UserIdKey, userId);
        session.SetString(RoleKey, role);
    }

    public static int? GetUserId(this ISession session) => session.GetInt32(UserIdKey);
    public static string? GetRole(this ISession session) => session.GetString(RoleKey);
    public static bool IsAuthenticated(this ISession session) => session.GetUserId() != null;
    public static bool IsInRole(this ISession session, string role) => session.GetRole() == role;
}
