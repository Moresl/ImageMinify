using System.Windows;

namespace ImageMinify;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new Views.MainWindow();
        MainWindow = window;
        window.Show();
    }
}
