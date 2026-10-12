namespace EmineoBlazor.Data;

public sealed record Product(
    string Slug,
    string Name,
    string ShortName,
    string Tagline,
    string Description,
    string NuGetPackage,
    string GitHubUrl,
    string DocsRoute,
    string ProductRoute,
    string Icon,
    string[] QuickWins,
    string PainPoint,
    string ValueProposition);

public static class Products
{
    public static readonly Product ResxTranslator = new("resx-translator", "NetAI.ResxTranslator", "ResxTranslator",
        "No more \"TODO: translate\" in your .resx files.",
        "Scans your resource files for missing translations and fills them in automatically — using your AI model, your languages, your rules.",
        "NetAI.ResxTranslator.Tasks", "https://github.com/emineo-net/NetAI.ResxTranslator", "/docs/resx-translator",
        "/products/resx-translator", "bi-globe", [
            "Detects missing keys automatically",
            "API key or local Ollama",
            "Domain-aware prompting",
            "Placeholders stay intact",
            "Auto-creates satellite files",
            "MSBuild + VS-safe"
        ],
        "keeping 12 languages in sync by hand — new keys land in English, the German file stays empty, and someone copies the English text just to make it compile",
        "fills missing .resx translations automatically during dotnet build — locally via Ollama, so no string ever leaves our machines");

    public static readonly Product TestGenerator = new("unittest-generator", "NetAI.TestGenerator", "TestGenerator",
        "The AI doesn't write the whole test. It fills a Roslyn-verified skeleton.",
        "Analyzes your classes with real Roslyn semantic analysis, builds a binding test skeleton, and lets the LLM fill only the test body — with compile validation and automatic repair.",
        "NetAI.TestGenerator.Tasks", "https://github.com/emineo-net/NetAI.TestGenerator", "/docs/test-generator",
        "/products/unittest-generator", "bi-eyedropper", [
            "Roslyn semantic analysis",
            "Compile validation + auto-repair",
            "xUnit · NUnit · MSTest",
            "Moq · NSubstitute · FakeItEasy",
            "WPF / STA-aware",
            "API key or local Ollama"
        ], "writing 400 test methods by hand and still missing coverage gates before every release",
        "generates real, compiling unit tests during dotnet build — Roslyn does the skeleton, the LLM fills only the test body, and every test is compile-validated before it lands");

    public static readonly Product[] All = [ResxTranslator, TestGenerator];

    public static Product? BySlug(string slug)
    {
        return All.FirstOrDefault(p => p.Slug == slug);
    }
}