using System.Collections.Generic;

namespace Promenade.Services.Abstract
{
    public interface ISocialService
    {
        bool IsSignValid(string userId, Dictionary<string, string> pars, string sign);
    }
}