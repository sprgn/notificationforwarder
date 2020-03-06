﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification_Forwarder.ConfigHelper
{
    public partial class AppInfo
    {
        public static bool Compare(AppInfo a, AppInfo b)
        {
            if (a.AppUserModelId != b.AppUserModelId) return false;
            if (a.DisplayName != b.DisplayName) return false;
            if (a.Id != b.Id) return false;
            return true;
        }
    }
}
