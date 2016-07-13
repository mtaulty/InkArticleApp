namespace InkArticleApp
{
  using Windows.ApplicationModel.Activation;
  using Windows.UI.Xaml;
  using Windows.UI.Xaml.Controls;
  sealed partial class App : Application
  {
    public App()
    {
      this.InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
      Frame rootFrame = Window.Current.Content as Frame;

      if (rootFrame == null)
      {
        // Create a Frame to act as the navigation context and navigate to the first page
        rootFrame = new Frame();

        // Place the frame in the current Window
        Window.Current.Content = rootFrame;
      }

      if (e.PrelaunchActivated == false)
      {
        if (rootFrame.Content == null)
        {
          rootFrame.Navigate(typeof(MainPage), e.Arguments);
        }
        // Ensure the current window is active
        Window.Current.Activate();
      }
    }
  }
}
