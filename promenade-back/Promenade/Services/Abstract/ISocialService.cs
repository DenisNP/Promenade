﻿using System.Collections.Generic;

namespace Promenade.Services.Abstract
{
    public interface ISocialService
    {
        void Notify(string[] userIds, string message, string hash = "");

        bool IsSignValid(string userId, Dictionary<string, string> pars, string sign);
    }
}