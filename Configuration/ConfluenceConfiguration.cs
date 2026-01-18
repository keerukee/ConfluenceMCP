namespace ConfluenceMcpServer.Configuration;

/// <summary>
/// Configuration for Confluence connection supporting both Datacenter and Cloud deployments.
/// </summary>
public static class ConfluenceConfiguration
{
    /// <summary>
    /// Confluence instance base URL (e.g., https://your-domain.atlassian.net/wiki or https://confluence.yourcompany.com)
    /// </summary>
    public static string BaseUrl => Environment.GetEnvironmentVariable("CONFLUENCE_BASE_URL") ?? "";

    /// <summary>
    /// Deployment type: "cloud" or "datacenter"
    /// </summary>
    public static string DeploymentType => Environment.GetEnvironmentVariable("CONFLUENCE_DEPLOYMENT_TYPE")?.ToLowerInvariant() ?? "cloud";

    /// <summary>
    /// For Cloud: Email address associated with the API token
    /// </summary>
    public static string Email => Environment.GetEnvironmentVariable("CONFLUENCE_EMAIL") ?? "";

    /// <summary>
    /// For Cloud: API token generated from Atlassian account
    /// For Datacenter: Personal Access Token (PAT)
    /// </summary>
    public static string ApiToken => Environment.GetEnvironmentVariable("CONFLUENCE_API_TOKEN") ?? "";

    /// <summary>
    /// Validates that required configuration is present
    /// </summary>
    public static (bool IsValid, string ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            return (false, "CONFLUENCE_BASE_URL environment variable is not set");

        if (string.IsNullOrWhiteSpace(ApiToken))
            return (false, "CONFLUENCE_API_TOKEN environment variable is not set");

        if (IsCloud && string.IsNullOrWhiteSpace(Email))
            return (false, "CONFLUENCE_EMAIL environment variable is required for Cloud deployment");

        return (true, string.Empty);
    }

    /// <summary>
    /// Returns true if configured for Confluence Cloud
    /// </summary>
    public static bool IsCloud => DeploymentType == "cloud";

    /// <summary>
    /// Returns true if configured for Confluence Datacenter
    /// </summary>
    public static bool IsDatacenter => DeploymentType == "datacenter";

    /// <summary>
    /// Gets the authorization header value based on deployment type
    /// </summary>
    public static string GetAuthorizationHeader()
    {
        if (IsCloud)
        {
            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{Email}:{ApiToken}"));
            return $"Basic {credentials}";
        }
        else
        {
            return $"Bearer {ApiToken}";
        }
    }

    /// <summary>
    /// Gets the API base path based on deployment type
    /// Cloud uses /wiki/api/v2 for newer endpoints
    /// Datacenter uses /rest/api for most operations
    /// </summary>
    public static string GetApiBasePath(bool useV2 = false)
    {
        if (IsCloud)
        {
            return useV2 ? "/wiki/api/v2" : "/wiki/rest/api";
        }
        else
        {
            return "/rest/api";
        }
    }
}
