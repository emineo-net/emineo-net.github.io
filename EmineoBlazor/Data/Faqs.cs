namespace EmineoBlazor.Data;

public sealed record FaqItem(string Question, string Answer);

public static class Faqs
{
    public static readonly FaqItem[] General =
    [
        new("What is Emineo?",
            "Emineo builds AI-powered NuGet packages for .NET teams. Both products â€” <strong>NetAI.ResxTranslator</strong> and <strong>NetAI.TestGenerator</strong> â€” run during <code>dotnet build</code>, use any OpenAI-compatible LLM (local or cloud), and are MIT-licensed at their core."),
        new("Do I need a cloud API key?",
            "No. Both products work fully locally via <strong>Ollama</strong>, <strong>LM Studio</strong>, <strong>llama.cpp</strong>, or our companion app <strong>LLMRunner</strong> (serves a model on localhost:8080 with one click). Cloud providers like OpenAI or Azure are optional â€” same configuration, your choice."),
        new("Is the community edition really MIT-licensed?",
            "Yes. The community core of both products is MIT-licensed, including commercial use. Enterprise editions are built on top of the same repositories and add team-scale features on top of the free core."),
        new("How do I get started?",
            "<code>dotnet add package NetAI.ResxTranslator.Tasks</code> (or <code>.TestGenerator.Tasks</code>), build once to scaffold <code>aisettings.json</code>, point it at your LLM, build again. Full walkthrough in the <a href=\"/docs/quickstart\">Quickstart</a>."),
    ];

    public static readonly FaqItem[] Enterprise =
    [
        new("Can we pay by invoice (purchase order)?",
            "Yes. For Enterprise and Bundle licenses we issue standard invoices with VAT â€” made out to your company name, with PO number and 30-day net payment terms. For individual purchases via Paddle, the invoice is available in your dashboard immediately after checkout."),
        new("Is there an MSA or DPA available?",
            "Yes. On request we provide a <strong>Master Service Agreement (MSA)</strong> and a <strong>Data Processing Agreement (DPA) per GDPR Art. 28</strong>. Because running with Ollama processes no personal data at all, an on-prem deployment often doesn't require a DPA â€” and when using cloud providers with your own API key, the data processing is between you and your chosen AI vendor."),
        new("Where is our data processed?",
            "You decide. <strong>1. Local (Ollama)</strong> â€” code and translations stay on your machine, no external calls. <strong>2. Cloud with your API key</strong> â€” requests go directly from your machine to your AI provider (OpenAI, Azure EU, Anthropic); we never see the data. <strong>3. Hybrid</strong> â€” sensitive resources locally, non-critical ones in the cloud, configurable per project or per file."),
        new("What happens after purchase â€” how do we get license keys?",
            "Within minutes you receive by email: the license key (<code>EMN-XXXX-XXXX-XXXX-XXXX</code>), access to the user dashboard with all licenses and invoices, an onboarding email, and â€” for Enterprise â€” a proposed slot for the 1:1 onboarding session."),
        new("Can we integrate into CI/CD?",
            "Yes. Both tools run headless during <code>dotnet build</code> / <code>dotnet publish</code> and read their configuration from <code>aisettings.json</code>. They slot directly into GitHub Actions, Azure DevOps, GitLab CI, and any generic MSBuild pipeline, and can act as a quality gate. For air-gapped setups, run Ollama in the runner â€” no byte leaves your network."),
        new("Which AI providers and models are supported?",
            "Any endpoint that speaks the OpenAI Chat Completions API: OpenAI, Azure OpenAI, Ollama, LM Studio, llama.cpp server, or your corporate gateway. Set <code>baseUrl</code>, <code>model</code>, and optionally <code>apiKey</code> or <code>apiKeyEnvVar</code>."),
    ];

    public static readonly FaqItem[] ResxTranslator =
    [
        new("Will existing translations be overwritten?",
            "No. Only entries with an empty <code>&lt;value&gt;</code> are translated. Everything else stays untouched â€” idempotent and incremental by design."),
        new("What about placeholders like {0} or %s?",
            "<code>{0}</code>, <code>{name}</code>, <code>%s</code>, <code>\\n</code>, <code>\\t</code> are treated as immutable by the prompt and validated on return. If the model changes a placeholder, the response is discarded."),
        new("Which languages are supported?",
            "Any culture code that <code>CultureInfo</code> recognizes. Declare <code>supportedLanguages</code> in <code>aisettings.json</code> and missing satellite files are created on the fly."),
        new("Does it run during every build?",
            "By default yes, but you control it. <code>translator.mode</code> accepts <code>all</code>, <code>debug</code>, <code>release</code>, <code>publish</code>, or combinations. <code>ResxTranslatorEnabled=false</code> disables it entirely."),
    ];

    public static readonly FaqItem[] TestGenerator =
    [
        new("Will the AI invent types or namespaces?",
            "No â€” that's the whole point. Roslyn builds a binding skeleton (usings, mock fields, constructor, attributes) from real symbols. The LLM only fills the test body. Anything outside the AI area is authoritative."),
        new("What if the generated test doesn't compile?",
            "Every test is compiled against your real test project before it's written. Missing usings are added by Roslyn, missing NuGet packages are resolved, and if all else fails the model gets one focused repair prompt with concrete hints."),
        new("Which test frameworks are supported?",
            "xUnit, NUnit, and MSTest, with Moq, NSubstitute, or FakeItEasy. FluentAssertions and AutoFixture are optional. WPF/STA is detected automatically."),
        new("Will it try to test async void methods?",
            "No. <code>async void</code> is detected and produces a skip stub with a concrete refactoring hint. Same for other untestable patterns (static dependencies, missing seams)."),
        new("Can it slow down my build?",
            "Only as much as the LLM does. Both products skip design-time builds, respect multi-targeting, and can be disabled per build. For prompt tuning there's <code>-p:TestGeneratorPromptOnly=true</code> (skips compile validation) and, for the CLI, <code>--dry-run</code> (no LLM call at all)."),
    ];
}