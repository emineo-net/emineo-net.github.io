namespace EmineoBlazor.Data;

public sealed record UseCase(string Icon, string Title, string Description);

public static class UseCases
{
    public static readonly UseCase[] ResxTranslator =
    [
        new("bi-globe", "Multi-tenant SaaS",
            "Ship in 12 languages without a dedicated localization team. New keys land in every culture before the PR is merged."),
        new("bi-brush", "Legacy .NET migrations",
            "Years of \"TODO: translate\" cleared in one pass. Audit reports show exactly what changed per culture."),
        new("bi-lock", "Regulated & air-gapped",
            "Run Ollama inside your network. Nothing leaves the machine, no cloud vendor ever sees your strings."),
    ];

    public static readonly UseCase[] TestGenerator =
    [
        new("bi-graph-up-arrow", "Legacy coverage rescue",
            "Take a project from 12% to 80% coverage without hand-writing hundreds of test methods."),
        new("bi-stoplights", "CI quality gates",
            "Fail the build when coverage drops or when new methods ship without tests. Zero-tolerance, zero-effort."),
        new("bi-book", "Onboarding & docs",
            "Generated tests double as executable documentation — new team members see exactly how the code is meant to be used."),
    ];

    public static UseCase[] ForProduct(string slug) => slug switch
    {
        "resx-translator" => ResxTranslator,
        "unittest-generator" => TestGenerator,
        _ => []
    };
}