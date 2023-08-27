using Modio;

namespace Anno.EasyMod.ModioWrapper
{
    public interface IModioClientProvider
    {
        Client? Client { get; }

        Task Authenticate();
        bool IsAuthenticated();
        void Logout();
    }
}