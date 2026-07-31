namespace X7.Knowledge.Publishing;

/// <summary>
/// Operações de diretório tolerantes a travas transitórias.
/// Clientes de sincronização (OneDrive, Google Drive, Dropbox), antivírus e
/// indexadores mantêm handles abertos por alguns instantes após a escrita.
/// A falha é temporária; falhar de primeira não é aceitável.
/// </summary>
internal static class ResilientDirectory
{
    private static readonly int[] BackoffMs = [50, 100, 200, 400, 800];

    /// <summary>Apaga com retentativa. Lança se não conseguir.</summary>
    public static void Delete(string path)
    {
        if (!Directory.Exists(path))
            return;

        Retry(() => Directory.Delete(path, recursive: true), path);
    }

    /// <summary>
    /// Tenta apagar; devolve falso se não conseguir. Não lança.
    /// Usado onde a falha não compromete o resultado.
    /// </summary>
    public static bool TryDelete(string path)
    {
        try
        {
            Delete(path);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public static void Move(string source, string destination)
        => Retry(() => Directory.Move(source, destination), destination);

    private static void Retry(Action action, string path)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                action();
                return;
            }
            catch (Exception e) when
                ((e is IOException or UnauthorizedAccessException)
                 && attempt < BackoffMs.Length)
            {
                Thread.Sleep(BackoffMs[attempt]);
            }
        }
    }
}
