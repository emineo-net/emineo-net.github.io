namespace ReadmeCopyCli;

internal class Program
{
    // 1. Das Dictionary mit Quell- und Zielpfaden definieren
    private static readonly Dictionary<string, string> DocMapping = new()
    {
        {
            @"C:\Users\steph\source\repos\NetAI.TestGenerator\README.md",
            @"C:\Users\steph\source\repos\emineo-net.github.io\EmineoBlazor\wwwroot\docs\test-generator.md"
        },
        {
            @"C:\Users\steph\source\repos\NetAI.TestGenerator\src\Libraries\NetAI.TestGenerator.Cli\README.md",
            @"C:\Users\steph\source\repos\emineo-net.github.io\EmineoBlazor\wwwroot\docs\test-generator-cli.md"
        },
        {
            @"C:\Users\steph\source\repos\NetAI.TestGenerator\src\Libraries\NetAI.TestGenerator.Tasks\README.md",
            @"C:\Users\steph\source\repos\emineo-net.github.io\EmineoBlazor\wwwroot\docs\test-generator-tasks.md"
        },
        {
            @"C:\Users\steph\source\repos\NetAI.ResxTranslator\README.md",
            @"C:\Users\steph\source\repos\emineo-net.github.io\EmineoBlazor\wwwroot\docs\resx-translator.md"
        },
        {
            @"C:\Users\steph\source\repos\NetAI.ResxTranslator\src\Libraries\NetAI.ResxTranslator.Tasks\README.md",
            @"C:\Users\steph\source\repos\emineo-net.github.io\EmineoBlazor\wwwroot\docs\resx-translator-tasks.md"
        },
        {
            @"C:\Users\steph\source\repos\NetAI.ResxTranslator\src\Libraries\NetAI.ResxTranslator.Cli\README.md",
            @"C:\Users\steph\source\repos\emineo-net.github.io\EmineoBlazor\wwwroot\docs\resx-translator-cli.md"
        }
    };

    private static void Main(string[] args)
    {
        Console.WriteLine("🔄 Starte Dokumenten-Synchronisation...");
        var successCount = 0;

        foreach (var kvp in DocMapping)
        {
            var sourcePath = kvp.Key;
            var targetPath = kvp.Value;

            try
            {
                // Prüfen, ob die Quell-README existiert
                if (!File.Exists(sourcePath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠️ Übersprungen: Quelle nicht gefunden -> {sourcePath}");
                    Console.ResetColor();
                    continue;
                }

                // Zielverzeichnis ermitteln und erstellen, falls es noch nicht existiert
                var targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // Datei kopieren (true = überschreiben, falls bereits vorhanden)
                File.Copy(sourcePath, targetPath, true);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Kopiert: {sourcePath} -> {targetPath}");
                Console.ResetColor();
                successCount++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Fehler bei {sourcePath}: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine($"\n✨ Fertig! {successCount} von {DocMapping.Count} Dateien erfolgreich synchronisiert.");
    }
}