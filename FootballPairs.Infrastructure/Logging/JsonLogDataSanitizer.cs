using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using FootballPairs.Application.Logging;

namespace FootballPairs.Infrastructure.Logging;

public sealed class JsonLogDataSanitizer : ILogDataSanitizer
{
    private const string Redacted = "***REDACTED***";
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "passwordHash",
        "passwordSalt",
        "iterations",
        "authorization",
        "cookie",
        "set-cookie",
        "x-api-key",
        "apiKey",
        "accessToken",
        "idToken",
        "refreshToken",
        "clientSecret",
        "token"
    };
    private static readonly string[] SensitiveKeyFragments =
    {
        "password",
        "secret",
        "token",
        "authorization",
        "cookie",
        "api-key",
        "apikey"
    };
    private static readonly Regex KeyValueSecretPattern = new(
        "(?i)\\b(password|passwordhash|passwordsalt|iterations|authorization|cookie|set-cookie|x-api-key|apikey|accesstoken|idtoken|refreshtoken|clientsecret|token)\\b\\s*[:=]\\s*([^\\s,&\\\"}]+)",
        RegexOptions.Compiled);
    private static readonly Regex BearerTokenPattern = new(
        @"(?i)\bbearer\s+[A-Za-z0-9\-\._~\+/]+=*",
        RegexOptions.Compiled);

    public string Sanitize(string jsonPayload)
    {
        var node = JsonNode.Parse(jsonPayload);
        if (node is null)
        {
            return jsonPayload;
        }

        RedactNode(node);
        return node.ToJsonString();
    }

    private static void RedactNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            var keys = jsonObject.Select(pair => pair.Key).ToArray();
            foreach (var key in keys)
            {
                if (IsSensitiveKey(key))
                {
                    jsonObject[key] = Redacted;
                    continue;
                }

                var child = jsonObject[key];
                if (child is not null)
                {
                    if (child is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var stringValue))
                    {
                        jsonObject[key] = SanitizeStringValue(stringValue);
                    }
                    else
                    {
                        RedactNode(child);
                    }
                }
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            for (var index = 0; index < jsonArray.Count; index++)
            {
                var child = jsonArray[index];
                if (child is null)
                {
                    continue;
                }

                if (child is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var stringValue))
                {
                    jsonArray[index] = SanitizeStringValue(stringValue);
                }
                else
                {
                    RedactNode(child);
                }
            }
        }
    }

    private static string SanitizeStringValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var sanitized = KeyValueSecretPattern.Replace(value, static match => $"{match.Groups[1].Value}=***REDACTED***");
        sanitized = BearerTokenPattern.Replace(sanitized, "Bearer ***REDACTED***");
        return sanitized;
    }

    private static bool IsSensitiveKey(string key)
    {
        return SensitiveKeys.Contains(key)
            || SensitiveKeyFragments.Any(fragment => key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
