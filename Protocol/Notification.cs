﻿using Notification_Forwarder.ConfigHelper;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Notifications;

namespace Notification_Forwarder.Protocol
{
    public partial class Notification
    {
        public AppInfo App;
        public string TimeStamp;
        public string Title;
        public string Content;
        public uint MessageId;

        public Notification() { }
        public Notification(AppInfo app, string timeStamp, string title, string content, uint id)
        {
            App = app;
            TimeStamp = timeStamp;
            Title = title;
            Content = content;
            MessageId = id;
        }
        public Notification(UserNotification source)
        {
            App = new AppInfo(source.AppInfo);
            TimeStamp = source.CreationTime.DateTime.ToString("o");
            MessageId = source.Id;
            var toastBinding = source.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (toastBinding != null)
            {
                IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();
                Title = textElements.FirstOrDefault()?.Text;
                Content = string.Join("\n", textElements.Skip(1).Select(t => t.Text));
            }
            else
            {
                Title = string.Empty;
                Content = string.Empty;
            }
        }
    }
}
