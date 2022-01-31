using Notification_Forwarder.ConfigHelper;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using System;

namespace Notification_Forwarder
{
    /// <summary>
    /// Provides application-specific behavior to complement the default application class.
    /// </summary>
    sealed partial class App : Application
    {
        private ExtendedExecutionForegroundSession newSession = null;

        /// <summary>
        /// Initializes the singleton application object. This is the first line of the authoring code executed,
        /// Executed, logically equivalent to main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            Debug.AutoFlush = true;
        }

        private void OnResuming(object sender, object e)
        {
            Conf.Log("RESUME TRIGGERED");
            if (!MainPage.IsUploadWorkerActive)
            {
                Conf.Log("app resumed, restoring upload worker...");
                MainPage.StartUploadWorker();
            }
        }

        /// <summary>
        /// Called when the application is normally started by the end user.
        /// Will be used when launching an application to open a specific file, etc.
        /// </summary>
        /// <param name="e">Details about the start request and process. </param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunchedContinueAsync(e);
        }

        private async void OnLaunchedContinueAsync(LaunchActivatedEventArgs e)
        {
            newSession = new ExtendedExecutionForegroundSession();
            newSession.Reason = ExtendedExecutionForegroundReason.Unconstrained;
            newSession.Description = "Long Running Processing";
            //newSession.Revoked += SessionRevoked;
            ExtendedExecutionForegroundResult result = await newSession.RequestExtensionAsync();
            switch (result)
            {
                case ExtendedExecutionForegroundResult.Allowed:
                    Conf.Log("Creating extended session.", LogLevel.Info);
                    OnLaunchedContinue(e);
                    break;

                default:
                case ExtendedExecutionForegroundResult.Denied:
                    Conf.Log("Not able to start extended session.", LogLevel.Error);
                    break;
            }
        }
        private void OnLaunchedContinue(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            Conf.Log("logs may not appear as soon as they're logged, refer to the timestamp as standard.", LogLevel.Warning);
            // read settings
            Conf.Log("reading settings...");
            _ = Conf.Read();
            Conf.Log("settings read successfully.", LogLevel.Complete);

            // don't repeat application initialization when the window already contains content,
            // just make sure the window is active
            if (rootFrame == null)
            {
                // Create the frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // put the frame in the current window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack has not been restored, navigate to the first page,
                    // and configure by passing in the required information as navigation parameters
                    // parameters


                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Make sure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Called when navigating to a specific page fails
        /// </summary>
        ///<param name="sender">Navigation failed frame</param>
        ///<param name="e">Details about navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Conf.Log($"unable to load page: {e.SourcePageType.FullName}.", LogLevel.Warning);
        }

        /// <summary>
        /// Called when application execution is about to be suspended. without knowing the application
        /// No need to know if the application will be terminated or resumed,
        /// and leave the memory contents unchanged.
        /// </summary>
        /// <param name="sender">The source of the pending request. </param>
        /// <param name="e">Details about pending requests. </param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // save conf
            try
            {
                Conf.Log("SUSPEND TRIGGERED");
                Debug.WriteLine("attempting to save data...");
                Conf.Log("saving data on suspension...");
                Conf.Save(Conf.CurrentConf);
            }
            catch { }
            deferral.Complete();
        }
    }
}
