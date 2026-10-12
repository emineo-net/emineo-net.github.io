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
    string ValueProposition
);

public static class Products
{
    public static readonly Product ResxTranslator = new(
        Slug: "resx-translator",
        Name: "NetAI.ResxTranslator",
        ShortName: "ResxTranslator",
        Tagline: "No more \"TODO: translate\" in your .resx files.",
        Description: "Scans your resource files for missing translations and fills them in automatically — using your AI model, your languages, your rules.",
        NuGetPackage: "NetAI.ResxTranslator.Tasks",
        GitHubUrl: "https://github.com/emineo-net/NetAI.ResxTranslator",
        DocsRoute: "/docs/resx-translator",
        ProductRoute: "/products/resx-translator",
        Icon: "bi-globe",
        QuickWins:
        [
            "Detects missing keys automatically",
            "API key or local Ollama",
            "Domain-aware prompting",
            "Placeholders stay intact",
            "Auto-creates satellite files",
            "MSBuild + VS-safe"
        ],
        PainPoint: "keeping 12 languages in sync by hand — new keys land in English, the German file stays empty, and someone copies the English text just to make it compile",
        ValueProposition: "fills missing .resx translations automatically during dotnet build — locally via Ollama, so no string ever leaves our machines"
    );

    public static readonly Product TestGenerator = new(
        Slug: "unittest-generator",
        Name: "NetAI.TestGenerator",
        ShortName: "TestGenerator",
        Tagline: "The AI doesn't write the whole test. It fills a Roslyn-verified skeleton.",
        Description: "Analyzes your classes with real Roslyn semantic analysis, builds a binding test skeleton, and lets the LLM fill only the test body — with compile validation and automatic repair.",
        NuGetPackage: "NetAI.TestGenerator.Tasks",
        GitHubUrl: "https://github.com/emineo-net/NetAI.TestGenerator",
        DocsRoute: "/docs/test-generator",
        ProductRoute: "/products/unittest-generator",
        Icon: "bi-eyedropper",
        QuickWins:
        [
            "Roslyn semantic analysis",
            "Compile validation + auto-repair",
            "xUnit · NUnit · MSTest",
            "Moq · NSubstitute · FakeItEasy",
            "WPF / STA-aware",
            "API key or local Ollama"
        ],
        PainPoint: "writing 400 test methods by hand and still missing coverage gates before every release",
        ValueProposition: "generates real, compiling unit tests during dotnet build — Roslyn does the skeleton, the LLM fills only the test body, and every test is compile-validated before it lands"
    );

    public static readonly Product[] All = [ResxTranslator, TestGenerator];

    public static Product? BySlug(string slug) =>
        All.FirstOrDefault(p => p.Slug == slug);
}