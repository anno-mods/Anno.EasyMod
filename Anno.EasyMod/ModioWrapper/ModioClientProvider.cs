using Microsoft.Extensions.Logging;
using Modio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.ModioWrapper
{
    public class ModioClientProvider : IModioClientProvider
    {
        private IModioAuthenticator _authenticator;
        private ILogger<ModioClientProvider> _logger;

        public ModioClientProvider(
            IModioAuthenticator authenticator,
            ILogger<ModioClientProvider> logger)
        {
            _authenticator = authenticator;
            _logger = logger;
        }

        public Modio.Client? Client { get; private set; }

        public async Task Authenticate()
        {
            Client = await _authenticator.CreateAuthenticatedAsync();


            if (Client is null)
            {
                _logger.LogError("Modio Authentication failed! Did not receive a Modio Client");
                return;
            }

            try
            {
                var user = await Client.User.GetCurrentUser();
                _logger.LogInformation("Authenticated on Modio as: " + user.Username);
            }
            catch (UnauthorizedException e) {
                _logger.LogError(e, "Modio Authentication failed! ");
                throw e; 
            }
        }

        public void Logout()
        {
            Client = null;
        }

        public bool IsAuthenticated()
        {
            return Client != null;
        }
    }
}
