namespace ChatApp.Core.Services.Interfaces
{
    public interface IAuthService
    {
        event Action<string> OnAuthSuccessful;
        event Action<string> OnAuthFailed;

        Task RegisterAsync(string username, string password);
        Task LoginAsync(string username, string password);
        Task LogoutAsync();
        bool IsAuthenticated { get; }
        string CurrentUsername { get; }
    }
}