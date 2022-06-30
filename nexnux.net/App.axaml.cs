using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using nexnux.net.ViewModels;
using nexnux.net.Views;

namespace nexnux.net
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new GameListView
                {
                    DataContext = new GameListViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}