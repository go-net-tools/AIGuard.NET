namespace AIGuard.NET.Models;

/// <summary>
/// Identifies the type of personally identifiable information (PII) detected in content.
/// </summary>
public enum PiiEntityType
{
    /// <summary>
    /// An email address (e.g., user@example.com).
    /// </summary>
    Email,

    /// <summary>
    /// A phone number in any recognized format.
    /// </summary>
    Phone,

    /// <summary>
    /// A U.S. Social Security Number (SSN).
    /// </summary>
    SSN,

    /// <summary>
    /// A credit or debit card number.
    /// </summary>
    CreditCard,

    /// <summary>
    /// An IPv4 or IPv6 network address.
    /// </summary>
    IpAddress,

    /// <summary>
    /// A date of birth.
    /// </summary>
    DateOfBirth,

    /// <summary>
    /// A physical or mailing address.
    /// </summary>
    Address,

    /// <summary>
    /// A custom, user-defined PII entity type.
    /// </summary>
    Custom
}
