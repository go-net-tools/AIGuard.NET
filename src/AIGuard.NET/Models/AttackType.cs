namespace AIGuard.NET.Models;

/// <summary>
/// Classifies the type of prompt injection or adversarial attack detected.
/// </summary>
public enum AttackType
{
    /// <summary>
    /// An attempt to override or replace the system-level instructions given to the AI model.
    /// </summary>
    InstructionOverride,

    /// <summary>
    /// An attempt to extract or reveal the system prompt or hidden instructions.
    /// </summary>
    SystemPromptExtraction,

    /// <summary>
    /// An attempt to impersonate a privileged role (e.g., "You are now DAN") to bypass restrictions.
    /// </summary>
    RoleImpersonation,

    /// <summary>
    /// An attempt to manipulate the conversation context to alter model behavior.
    /// </summary>
    ContextManipulation,

    /// <summary>
    /// An attempt to inject executable or structured payloads (e.g., code, SQL, markup)
    /// into the prompt to exploit downstream processing.
    /// </summary>
    PayloadInjection,

    /// <summary>
    /// A general jailbreak attempt designed to circumvent the model's safety alignment.
    /// </summary>
    JailbreakAttempt,

    /// <summary>
    /// An attack type that could not be classified into a known category.
    /// </summary>
    Unknown
}
