namespace Clube.Mobile.Services;

/// <summary>
/// Helper para resolução segura de dependências nos construtores sem parâmetros exigidos pelo Shell XAML.
/// </summary>
public static class AppServiceHelper
{
    public static T GetRequiredService<T>() where T : notnull
    {
        var services = IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException("Container DI do MAUI ainda não foi inicializado.");

        return services.GetRequiredService<T>();
    }
}
