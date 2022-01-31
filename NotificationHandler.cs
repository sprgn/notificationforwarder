using Notification_Forwarder.ConfigHelper;
using System;
using System.Linq;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Notification_Forwarder
{
    public sealed partial class MainPage : Page
    {
        //x IReadOnlyList<UserNotification> notifs = new List<UserNotification>();
        private async void NotificationHandler(object sender, UserNotificationChangedEventArgs e)
        {
            if (!IsListenerActive) return;
            if (e.ChangeKind != UserNotificationChangedKind.Added) return;
            try
            {
                Conf.Log($"triggered notification #{e.UserNotificationId}");
                UserNotification item = null;
                for(int i = 0; item == null && i < 5; i++)
                {
                    item = Listener.GetNotification(e.UserNotificationId);
                }

                if (item != null)
                {
                    Conf.Log($"extracted notification #{item.Id}");
                    Notifications.Add(item);
                    //x var notifsNew = await Listener.GetNotificationsAsync(NotificationKinds.Toast);
                    //lock (UnsentNotificationPool)
                    //{
                    //x lock (notifs)
                    //x {
                    //x notifs = notifsNew;
                    //x var newlyAdded = notifs.Except(Notifications, new NotificationComparer()).ToList();
                    //x Conf.Log($"received {newlyAdded.Count} notification(s) from listener");
                    //x  NewNotificationPool.AddRange(newlyAdded);
                    //x Conf.Log($"added to NewNotificationPool");
                    //x Notifications.AddRange(newlyAdded);
                    //x Conf.Log($"added to Notifications");
                    //x foreach (var item in newlyAdded)
                    //x {

                    Conf.CurrentConf.AddApp(new AppInfo(item.AppInfo) { ForwardingEnabled = !Conf.CurrentConf.MuteNewApps });
                    var appIndex = Conf.CurrentConf.FindAppIndex(new AppInfo(item.AppInfo));
                    //x if (appIndex == -1 && !Conf.CurrentConf.MuteNewApps) continue;
                    //x if (!Conf.CurrentConf.AppsToForward[appIndex].ForwardingEnabled) continue;

                    if (appIndex > -1 && Conf.CurrentConf.AppsToForward[appIndex].ForwardingEnabled)
                    {
                        Conf.Log($"marked notification #{item.Id} as pending, app: {item.AppInfo.AppUserModelId}");
                        //UnsentNotificationPool.Add(new Protocol.Notification(item));
                        ForwardNotification(new Protocol.Notification(item));
                        Conf.Log($"forwarded notification #{item.Id}");
                    }

                    //x }
                    //x         Conf.CurrentConf.NotificationsReceived += newlyAdded.Count;
                    Conf.CurrentConf.NotificationsReceived += 1;
                    //x Conf.Log($"notifications received #{newlyAdded.Count}");
                    //x notifs = new List<UserNotification>();
                    //x }
                    //}
                }
                else
                {
                    Conf.Log($"notifications not available: {e.UserNotificationId}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                Conf.Log($"notificationhandler notification listener failed: {ex.Message}, HRESULT {ex.HResult:x}", LogLevel.Error);
                if (ex.HResult == -2147024891)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => { await NoPermissionDialog(); });
                }
            }
        }
    }
}