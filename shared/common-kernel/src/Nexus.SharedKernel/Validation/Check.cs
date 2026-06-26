namespace Nexus.SharedKernel.Validation;

public static class Check
{
    public static string NotNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
        }

        return value;
    }

    public static string Length(string value, string parameterName, int maxLength, int minLength = 0)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            throw new ArgumentException($"{parameterName} length must be between {minLength} and {maxLength}.", parameterName);
        }

        return value;
    }
}
