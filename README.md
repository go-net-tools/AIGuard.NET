# AIGuard.NET 🛡️

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/AIGuard.NET.svg)](https://www.nuget.org/packages/AIGuard.NET)

**AIGuard.NET** is a powerful, extensible open-source developer utility for building safe, reliable, and predictable AI applications in .NET. It provides composable guardrails, schema enforcement, hallucination detection, and an automatic self-correction engine for Large Language Models (LLMs).

Unlike Python-first tools, AIGuard.NET is built from the ground up for the .NET 9 ecosystem, featuring seamless ASP.NET Core Dependency Injection support and native C# Type validation.

---

## 🌟 Key Features

*   **🛡️ Comprehensive Guardrails**: Protect against Prompt Injection, PII leakage, Toxic content, and Token limit exhaustion.
*   **📐 Strict Schema Enforcement**: Ensure LLM outputs perfectly match your C# `class` or `record` models, complete with `DataAnnotations` validation.
*   **🔁 Auto-Retry Engine**: The true differentiator. If an LLM hallucinates or outputs invalid data, AIGuard.NET automatically intercepts the failure, builds a targeted correction prompt, and forces the LLM to fix its own mistake.
*   **👩‍⚖️ LLM-as-a-Judge**: Built-in `HallucinationValidator` and `FactCheckingValidator` cross-reference outputs against your source documents to prevent hallucinations.
*   **🔌 Extensible Pipeline**: Easily build and plug in your own `IGuardValidator` implementations into the `GuardPipeline`.

---

## 📦 Installation

Available on [NuGet](https://www.nuget.org/packages/AIGuard.NET/):
```bash
dotnet add package AIGuard.NET
dotnet add package AIGuard.NET.AspNetCore
```

---

## 🚀 Quick Start

### 1. ASP.NET Core Integration

AIGuard.NET provides a fluent API that integrates perfectly with your `Program.cs`:

```csharp
using AIGuard.NET.AspNetCore.DependencyInjection;
using AIGuard.NET.Validators.Input;
using AIGuard.NET.Validators.Output;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAIGuard(guard => 
{
    // Configure input safety
    guard.AddValidator<PromptInjectionValidator>(v => v.Sensitivity = SensitivityLevel.High)
         .AddValidator<PiiRedactionValidator>()
         .AddValidator<TokenBudgetValidator>(v => v.MaxTokens = 4096);

    // Configure output safety
    guard.AddValidator<ToxicityValidator>()
         .AddValidator<TypeSchemaValidator<MyExpectedOutput>>();

    // Global settings
    guard.WithAuditLog(true)
         .WithStopOnFirstFailure(false);
});

var app = builder.Build();
```

### 2. The Auto-Retry Engine

The `RetryEngine` wraps your LLM calls. If the LLM generates a toxic response, leaks PII, or breaks your JSON schema, the engine automatically tells the LLM what it did wrong and asks for a correction.

```csharp
// 1. Inject the configured AIGuard and your custom ILlmClient
var guard = serviceProvider.GetRequiredService<AIGuard>();
var llmClient = new MyOpenAIClient(); 

var engine = new RetryEngine(llmClient, guard);

// 2. Execute with automatic self-correction (max 3 retries)
var result = await engine.ExecuteWithRetryAsync("Extract the user's data into JSON.");

if (result.IsSuccess)
{
    Console.WriteLine("Safe, Validated Output: " + result.FinalOutput);
}
else
{
    Console.WriteLine("Validation ultimately failed after 3 retries.");
    foreach (var violation in result.FinalValidation.AllViolations)
    {
        Console.WriteLine($" - [{violation.Rule}]: {violation.Description}");
    }
}
```

### 3. LLM-as-a-Judge (Hallucination Detection)

Ensure your AI's answers are grounded in your actual proprietary documents using the `HallucinationValidator`:

```csharp
var validator = new HallucinationValidator(llmClient);

var context = new ValidationContext 
{
    SourceDocuments = ["The company was founded in 2026.", "Our main product is AIGuard."]
};

var validationResult = await validator.ValidateAsync("The company was founded in 1999.", context, CancellationToken.None);

// Result: FAIL - "Hallucination detected in output"
```

---

## 🛠️ Built-In Validators

### Input Validators
*   **`PromptInjectionValidator`**: Uses layered pattern matching and heuristics to stop jailbreaks (e.g., "DAN mode", "ignore all previous instructions").
*   **`PiiRedactionValidator`**: Redacts Emails, SSNs, Credit Cards, and IP Addresses.
*   **`TokenBudgetValidator`**: Prevents enormous prompts from causing bill shock.

### Output Validators
*   **`TypeSchemaValidator<T>`**: Validates JSON output structure and checks C# DataAnnotations.
*   **`ToxicityValidator`**: Detects hate speech, violence, and self-harm.
*   **`HallucinationValidator`**: (LLM-as-a-judge) Verifies output against source context.
*   **`FactCheckingValidator`**: (LLM-as-a-judge) Cross-references factual claims.
*   **`ResponseLengthValidator`**: Enforces word/character count boundaries.

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
