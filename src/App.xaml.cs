using System;
using System.IO;
using System.Windows;

namespace F1GridEditorApp;

public partial class App : System.Windows.Application
{
    public App()
    {
        this.DispatcherUnhandledException += (s, e) =>
        {
            try
            {
                File.WriteAllText("crash.log", $"DispatcherUnhandledException: {e.Exception}\n{e.Exception.InnerException}");
            }
            catch {}
            System.Windows.MessageBox.Show($"Error en la aplicación:\n{e.Exception.Message}\n\nDetalles guardados en crash.log", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            try
            {
                File.WriteAllText("crash_domain.log", $"UnhandledException: {e.ExceptionObject}");
            }
            catch {}
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            File.WriteAllText("crash.log", $"OnStartup Exception: {ex}");
            System.Windows.MessageBox.Show($"Error al iniciar MainWindow:\n{ex.Message}\n\nDetalles guardados en crash.log", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
