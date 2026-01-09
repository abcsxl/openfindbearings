using System.Text.RegularExpressions;

namespace OpenFindBearings.Shared.Domain.ValueObjects;

/// <summary>
/// 手机号码值对象
/// </summary>
public class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(@"^1[3-9]\d{9}$", RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("手机号码不能为空", nameof(value));

        var cleanedValue = value.Replace("-", "").Replace(" ", "");

        if (!PhoneRegex.IsMatch(cleanedValue))
            throw new ArgumentException("手机号码格式不正确", nameof(value));

        Value = cleanedValue;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    public static implicit operator PhoneNumber(string phoneNumber) => new(phoneNumber);

    public override string ToString() => Value;
}
