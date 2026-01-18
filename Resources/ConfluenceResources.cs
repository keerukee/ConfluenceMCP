using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using ConfluenceMcpServer.Configuration;
using ConfluenceMcpServer.Services;
using MCPServer.Attributes;

namespace ConfluenceMcpServer.Resources;

/// <summary>
/// MCP Resources for Confluence - providing read-only data access.
/// </summary>
public static class ConfluenceResources
{
    #region Configuration Resources

    [McpResource("confluence://config", "Confluence Configuration", "Returns current Confluence connection configuration (without sensitive data)", "application/json")]
    public static string GetConfiguration()
    {
        var config = new
        {
            baseUrl = ConfluenceConfiguration.BaseUrl,
            deploymentType = ConfluenceConfiguration.DeploymentType,
            isCloud = ConfluenceConfiguration.IsCloud,
            isDatacenter = ConfluenceConfiguration.IsDatacenter,
            isConfigured = ConfluenceConfiguration.Validate().IsValid,
            apiBasePath = ConfluenceConfiguration.GetApiBasePath()
        };
        return JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
    }

    #endregion

    #region Page Resources

    [McpResourceTemplate("confluence://page/{pageId}", "Confluence Page", "Returns details of a specific Confluence page", "application/json")]
    public static async Task<string> GetPageResource(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}?expand=body.storage,version,space,ancestors,metadata.labels");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://page/{pageId}/body", "Confluence Page Body", "Returns only the body content of a Confluence page", "text/html")]
    public static async Task<string> GetPageBody(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}?expand=body.storage");
            var json = JsonNode.Parse(response);
            var body = json?["body"]?["storage"]?["value"]?.GetValue<string>();
            return body ?? "";
        }
        catch (Exception ex)
        {
            return $"<!-- Error: {ex.Message} -->";
        }
    }

    [McpResourceTemplate("confluence://page/{pageId}/children", "Page Children", "Returns child pages of a Confluence page", "application/json")]
    public static async Task<string> GetPageChildrenResource(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/child/page?expand=version&limit=50");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://page/{pageId}/comments", "Page Comments", "Returns comments on a Confluence page", "application/json")]
    public static async Task<string> GetPageCommentsResource(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/child/comment?expand=body.storage,version&depth=all&limit=50");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://page/{pageId}/attachments", "Page Attachments", "Returns attachments on a Confluence page", "application/json")]
    public static async Task<string> GetPageAttachmentsResource(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/child/attachment?expand=version,metadata.mediaType&limit=50");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://page/{pageId}/labels", "Page Labels", "Returns labels on a Confluence page", "application/json")]
    public static async Task<string> GetPageLabelsResource(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/label");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://page/{pageId}/history", "Page History", "Returns version history of a Confluence page", "application/json")]
    public static async Task<string> GetPageHistoryResource(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/history?expand=lastUpdated,previousVersion,contributors");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Space Resources

    [McpResource("confluence://spaces", "All Spaces", "Returns list of all accessible Confluence spaces", "application/json")]
    public static async Task<string> GetAllSpaces()
    {
        try
        {
            var response = await ConfluenceClient.GetAsync("space?limit=100&expand=description.plain,homepage");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://space/{spaceKey}", "Confluence Space", "Returns details of a specific Confluence space", "application/json")]
    public static async Task<string> GetSpaceResource(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}?expand=description.plain,homepage,metadata.labels");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://space/{spaceKey}/pages", "Space Pages", "Returns all pages in a Confluence space", "application/json")]
    public static async Task<string> GetSpacePages(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}/content/page?limit=100&expand=version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://space/{spaceKey}/blogposts", "Space Blog Posts", "Returns all blog posts in a Confluence space", "application/json")]
    public static async Task<string> GetSpaceBlogPosts(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}/content/blogpost?limit=50&expand=version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://space/{spaceKey}/root-pages", "Space Root Pages", "Returns top-level pages in a Confluence space", "application/json")]
    public static async Task<string> GetSpaceRootPages(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}/content/page?depth=root&limit=50&expand=version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region User Resources

    [McpResource("confluence://current-user", "Current User", "Returns information about the authenticated user", "application/json")]
    public static async Task<string> GetCurrentUser()
    {
        try
        {
            var response = await ConfluenceClient.GetAsync("user/current");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResource("confluence://my-recent-work", "My Recent Work", "Returns content recently modified by current user", "application/json")]
    public static async Task<string> GetMyRecentWork()
    {
        try
        {
            var cql = HttpUtility.UrlEncode("contributor=currentUser() ORDER BY lastmodified DESC");
            var response = await ConfluenceClient.GetAsync($"search?cql={cql}&limit=25&expand=content.space");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Search Resources

    [McpResourceTemplate("confluence://search/{query}", "Search Results", "Returns search results for a query", "application/json")]
    public static async Task<string> SearchResource(
        [McpParameter("Search query")] string query)
    {
        try
        {
            var cql = HttpUtility.UrlEncode($"text~\"{query}\"");
            var response = await ConfluenceClient.GetAsync($"search?cql={cql}&limit=25&expand=content.space,excerpt");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://label/{label}", "Content by Label", "Returns content with a specific label", "application/json")]
    public static async Task<string> GetContentByLabel(
        [McpParameter("Label name")] string label)
    {
        try
        {
            var cql = HttpUtility.UrlEncode($"label=\"{label}\"");
            var response = await ConfluenceClient.GetAsync($"search?cql={cql}&limit=50&expand=content.space,content.metadata.labels");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Recent Content Resources

    [McpResource("confluence://recent-pages", "Recent Pages", "Returns recently modified pages across all spaces", "application/json")]
    public static async Task<string> GetRecentPages()
    {
        try
        {
            var cql = HttpUtility.UrlEncode("type=page ORDER BY lastmodified DESC");
            var response = await ConfluenceClient.GetAsync($"search?cql={cql}&limit=25&expand=content.space,content.version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResource("confluence://recent-blogposts", "Recent Blog Posts", "Returns recently published blog posts", "application/json")]
    public static async Task<string> GetRecentBlogPosts()
    {
        try
        {
            var cql = HttpUtility.UrlEncode("type=blogpost ORDER BY created DESC");
            var response = await ConfluenceClient.GetAsync($"search?cql={cql}&limit=25&expand=content.space,content.version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion

    #region Template Resources

    [McpResource("confluence://templates", "Global Templates", "Returns available global content templates", "application/json")]
    public static async Task<string> GetGlobalTemplates()
    {
        try
        {
            var response = await ConfluenceClient.GetAsync("template/page?expand=body&limit=50");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [McpResourceTemplate("confluence://space/{spaceKey}/templates", "Space Templates", "Returns content templates in a space", "application/json")]
    public static async Task<string> GetSpaceTemplates(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"template/page?spaceKey={spaceKey}&expand=body&limit=50");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    #endregion
}
