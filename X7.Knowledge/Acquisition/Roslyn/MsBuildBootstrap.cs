using Microsoft.Build.Locator;

namespace X7.Knowledge.Acquisition.Roslyn;

/// <summary>
/// Registra o SDK do MSBuild antes que qualquer tipo do MSBuild seja carregado.
/// </summary>
/// <remarks>
/// Restrição do MSBuildLocator, não escolha de design: o registro precisa
/// acontecer antes do primeiro uso, e uma vez só por processo. Por isso é
/// idempotente e silencioso quando não há SDK — nesse caso a compilação
/// segue em nível X, com a limitação declarada.
/// </remarks>
public static class MsBuildBootstrap
{
    private static readonly Lock Gate = new();

    private static bool _attempted;

    public static bool Registered { get; private set; }

    public static string? Version { get; private set; }

    public static string? Failure { get; private set; }

    public static bool Ensure()
    {
        lock (Gate)
        {
            if (_attempted)
                return Registered;

            _attempted = true;

            try
            {
                if (MSBuildLocator.IsRegistered)
                {
                    Registered = true;
                    return true;
                }

                // Ordenação explícita: a escolha do SDK não pode depender da
                // ordem em que o Locator resolve instalações (PR-02).
                var instance = MSBuildLocator
                    .QueryVisualStudioInstances()
                    .OrderByDescending(i => i.Version)
                    .ThenBy(i => i.MSBuildPath, StringComparer.Ordinal)
                    .FirstOrDefault();

                if (instance is null)
                {
                    Failure = "Nenhuma instalação do SDK .NET encontrada.";
                    return false;
                }

                MSBuildLocator.RegisterInstance(instance);

                Registered = true;
                Version = instance.Version.ToString();

                return true;
            }
            catch (Exception e) when (e is InvalidOperationException or FileNotFoundException)
            {
                Failure = e.Message;
                return false;
            }
        }
    }
}
