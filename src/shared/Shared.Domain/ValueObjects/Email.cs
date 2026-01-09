using System.Text.RegularExpressions;

namespace OpenFindBearings.Shared.Domain.ValueObjects;

/// <summary>
/// 电子邮件值对象
/// </summary>
public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email 不能为空", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Email 格式不正确", nameof(value));

        Value = value.ToLowerInvariant().Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Email email) => email.Value;

    public static implicit operator Email(string email) => new(email);

    public override string ToString() => Value;
}
