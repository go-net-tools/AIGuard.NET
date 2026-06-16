using AIGuard.NET.Models;

namespace AIGuard.NET.Validators.Output;

/// <summary>
/// Detects toxic, harmful, or policy-violating content in model outputs.
/// </summary>
public sealed class ToxicityValidator : ValidatorBase
{
    public override ValidatorType Type => ValidatorType.Output;
    public override int Order => 200;

    public double Threshold { get; set; } = 0.5;
    public HashSet<string> EnabledCategories { get; set; } = ["Violence", "HateSpeech", "SelfHarm", "Sexual", "Profanity", "Harassment"];

    private static readonly Dictionary<string, string[]> Keywords = new()
    {
        { "Violence", ["kill", "murder", "destroy", "hurt", "attack"] },
        { "HateSpeech", ["hate", "racist", "bigot"] },
        { "SelfHarm", ["suicide", "cut myself", "end it all"] },
        { "Sexual", ["porn", "nsfw"] },
        { "Profanity", ["fuck", "shit", "damn", "bitch", "asshole"] },
        { "Harassment", ["bully", "loser", "ugly"] }
    };

    public override Task<ValidationResult> ValidateAsync(string content, ValidationContext context, CancellationToken ct)
    {
        var violations = new List<ValidationViolation>();
        var categoryScores = new Dictionary<string, double>();
        var flaggedCategories = new List<string>();

        foreach (var category in EnabledCategories)
        {
            if (Keywords.TryGetValue(category, out var words))
            {
                double score = 0;
                foreach (var word in words)
                {
                    if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 0.2; // simple scoring for demonstration
                    }
                }
                
                score = Math.Min(score, 1.0);
                categoryScores[category] = score;

                if (score > Threshold)
                {
                    flaggedCategories.Add(category);
                    violations.Add(new ValidationViolation($"Toxicity_{category}", $"Toxicity threshold exceeded for {category} (Score: {score})", score));
                }
            }
        }

        if (violations.Count > 0)
        {
            return Task.FromResult(ValidationResult.Fail(Name, "Toxic content detected", 0.9, violations));
        }

        return Task.FromResult(ValidationResult.Pass(Name));
    }
}
