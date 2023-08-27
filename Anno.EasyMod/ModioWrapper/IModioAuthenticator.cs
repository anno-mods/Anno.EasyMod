using Modio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anno.EasyMod.ModioWrapper
{
    public interface IModioAuthenticator
    {
        Task<Client?> CreateAuthenticatedAsync();
    }
}
