using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using ConfluenceMcpServer.Configuration;
using ConfluenceMcpServer.Services;
using MCPServer.Attributes;

namespace ConfluenceMcpServer.Tools;

/// <summary>
/// MCP Tools for Confluence operations - Pages, Spaces, Search, Comments, Attachments, and more.
/// </summary>
public static class ConfluenceTools
{
    #region Page Operations

    [McpTool("confluence_get_page", "Get a Confluence page by ID")]
    public static async Task<string> GetPage(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Include page body content (default: true)", required: false)] bool includeBody = true,
        [McpParameter("Body format: storage, atlas_doc_format, view, export_view (default: storage)", required: false)] string bodyFormat = "storage")
    {
        try
        {
            var expand = includeBody ? $"body.{bodyFormat},version,space,ancestors" : "version,space,ancestors";
            var response = await ConfluenceClient.GetAsync($"content/{pageId}?expand={expand}");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting page: {ex.Message}";
        }
    }

    [McpTool("confluence_get_page_by_title", "Get a Confluence page by its title and space key")]
    public static async Task<string> GetPageByTitle(
        [McpParameter("The space key (e.g., DEV, HR)")] string spaceKey,
        [McpParameter("The page title")] string title)
    {
        try
        {
            var encodedTitle = HttpUtility.UrlEncode(title);
            var response = await ConfluenceClient.GetAsync($"content?spaceKey={spaceKey}&title={encodedTitle}&expand=body.storage,version,space,ancestors");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting page by title: {ex.Message}";
        }
    }

    [McpTool("confluence_create_page", "Create a new Confluence page")]
    public static async Task<string> CreatePage(
        [McpParameter("The space key where the page will be created")] string spaceKey,
        [McpParameter("The page title")] string title,
        [McpParameter("The page content in Confluence storage format (XHTML)")] string content,
        [McpParameter("Parent page ID (optional - creates as child page)", required: false)] string? parentId = null,
        [McpParameter("Content type: page or blogpost (default: page)", required: false)] string contentType = "page")
    {
        try
        {
            var pageData = new Dictionary<string, object>
            {
                ["type"] = contentType,
                ["title"] = title,
                ["space"] = new { key = spaceKey },
                ["body"] = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(parentId))
            {
                pageData["ancestors"] = new[] { new { id = parentId } };
            }

            var response = await ConfluenceClient.PostAsync("content", pageData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error creating page: {ex.Message}";
        }
    }

    [McpTool("confluence_update_page", "Update an existing Confluence page")]
    public static async Task<string> UpdatePage(
        [McpParameter("The page ID to update")] string pageId,
        [McpParameter("The new page title")] string title,
        [McpParameter("The new page content in Confluence storage format (XHTML)")] string content,
        [McpParameter("Current version number (required for update)")] int versionNumber,
        [McpParameter("Version message/comment (optional)", required: false)] string? versionMessage = null)
    {
        try
        {
            var pageData = new Dictionary<string, object>
            {
                ["id"] = pageId,
                ["type"] = "page",
                ["title"] = title,
                ["body"] = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                },
                ["version"] = new
                {
                    number = versionNumber + 1,
                    message = versionMessage ?? "Updated via MCP"
                }
            };

            var response = await ConfluenceClient.PutAsync($"content/{pageId}", pageData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error updating page: {ex.Message}";
        }
    }

    [McpTool("confluence_delete_page", "Delete a Confluence page")]
    public static async Task<string> DeletePage(
        [McpParameter("The page ID to delete")] string pageId,
        [McpParameter("Purge permanently (true) or move to trash (false, default)", required: false)] bool purge = false)
    {
        try
        {
            var endpoint = purge ? $"content/{pageId}?status=trashed" : $"content/{pageId}";
            await ConfluenceClient.DeleteAsync(endpoint);
            return $"Page {pageId} deleted successfully";
        }
        catch (Exception ex)
        {
            return $"Error deleting page: {ex.Message}";
        }
    }

    [McpTool("confluence_get_page_children", "Get child pages of a Confluence page")]
    public static async Task<string> GetPageChildren(
        [McpParameter("The parent page ID")] string pageId,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25,
        [McpParameter("Start index for pagination (default: 0)", required: false)] int start = 0)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/child/page?limit={limit}&start={start}&expand=version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting child pages: {ex.Message}";
        }
    }

    [McpTool("confluence_get_page_ancestors", "Get ancestor (parent) pages of a Confluence page")]
    public static async Task<string> GetPageAncestors(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}?expand=ancestors");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting page ancestors: {ex.Message}";
        }
    }

    [McpTool("confluence_get_page_history", "Get version history of a Confluence page")]
    public static async Task<string> GetPageHistory(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/history?expand=lastUpdated,previousVersion,contributors&limit={limit}");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting page history: {ex.Message}";
        }
    }

    [McpTool("confluence_get_page_version", "Get a specific version of a Confluence page")]
    public static async Task<string> GetPageVersion(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("The version number")] int versionNumber)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}?status=historical&version={versionNumber}&expand=body.storage,version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting page version: {ex.Message}";
        }
    }

    [McpTool("confluence_copy_page", "Copy a Confluence page to a new location")]
    public static async Task<string> CopyPage(
        [McpParameter("The source page ID to copy")] string sourcePageId,
        [McpParameter("Destination space key")] string destinationSpaceKey,
        [McpParameter("New page title")] string newTitle,
        [McpParameter("Parent page ID in destination (optional)", required: false)] string? parentId = null)
    {
        try
        {
            // First get the source page content
            var sourcePage = await ConfluenceClient.GetAsync($"content/{sourcePageId}?expand=body.storage");
            var sourceJson = JsonNode.Parse(sourcePage);
            var content = sourceJson?["body"]?["storage"]?["value"]?.GetValue<string>() ?? "";

            // Create the copy
            var pageData = new Dictionary<string, object>
            {
                ["type"] = "page",
                ["title"] = newTitle,
                ["space"] = new { key = destinationSpaceKey },
                ["body"] = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(parentId))
            {
                pageData["ancestors"] = new[] { new { id = parentId } };
            }

            var response = await ConfluenceClient.PostAsync("content", pageData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error copying page: {ex.Message}";
        }
    }

    [McpTool("confluence_move_page", "Move a Confluence page to a new parent")]
    public static async Task<string> MovePage(
        [McpParameter("The page ID to move")] string pageId,
        [McpParameter("New parent page ID")] string newParentId)
    {
        try
        {
            // Get current page details
            var currentPage = await ConfluenceClient.GetAsync($"content/{pageId}?expand=version,body.storage");
            var pageJson = JsonNode.Parse(currentPage);

            var title = pageJson?["title"]?.GetValue<string>() ?? "";
            var content = pageJson?["body"]?["storage"]?["value"]?.GetValue<string>() ?? "";
            var version = pageJson?["version"]?["number"]?.GetValue<int>() ?? 1;

            var pageData = new Dictionary<string, object>
            {
                ["id"] = pageId,
                ["type"] = "page",
                ["title"] = title,
                ["ancestors"] = new[] { new { id = newParentId } },
                ["body"] = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                },
                ["version"] = new { number = version + 1 }
            };

            var response = await ConfluenceClient.PutAsync($"content/{pageId}", pageData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error moving page: {ex.Message}";
        }
    }

    #endregion

    #region Space Operations

    [McpTool("confluence_list_spaces", "List all Confluence spaces")]
    public static async Task<string> ListSpaces(
        [McpParameter("Space type filter: global, personal, all (default: all)", required: false)] string type = "all",
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25,
        [McpParameter("Start index for pagination (default: 0)", required: false)] int start = 0)
    {
        try
        {
            var endpoint = $"space?limit={limit}&start={start}&expand=description.plain,homepage";
            if (type != "all")
                endpoint += $"&type={type}";

            var response = await ConfluenceClient.GetAsync(endpoint);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error listing spaces: {ex.Message}";
        }
    }

    [McpTool("confluence_get_space", "Get details of a Confluence space")]
    public static async Task<string> GetSpace(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}?expand=description.plain,homepage,metadata.labels");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting space: {ex.Message}";
        }
    }

    [McpTool("confluence_create_space", "Create a new Confluence space")]
    public static async Task<string> CreateSpace(
        [McpParameter("The space key (unique identifier, e.g., DEV)")] string spaceKey,
        [McpParameter("The space name")] string name,
        [McpParameter("Space description (optional)", required: false)] string? description = null,
        [McpParameter("Space type: global or personal (default: global)", required: false)] string type = "global")
    {
        try
        {
            var spaceData = new Dictionary<string, object>
            {
                ["key"] = spaceKey.ToUpperInvariant(),
                ["name"] = name,
                ["type"] = type
            };

            if (!string.IsNullOrWhiteSpace(description))
            {
                spaceData["description"] = new
                {
                    plain = new { value = description, representation = "plain" }
                };
            }

            var response = await ConfluenceClient.PostAsync("space", spaceData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error creating space: {ex.Message}";
        }
    }

    [McpTool("confluence_delete_space", "Delete a Confluence space")]
    public static async Task<string> DeleteSpace(
        [McpParameter("The space key to delete")] string spaceKey)
    {
        try
        {
            await ConfluenceClient.DeleteAsync($"space/{spaceKey}");
            return $"Space {spaceKey} deleted successfully";
        }
        catch (Exception ex)
        {
            return $"Error deleting space: {ex.Message}";
        }
    }

    [McpTool("confluence_get_space_content", "Get all content in a space")]
    public static async Task<string> GetSpaceContent(
        [McpParameter("The space key")] string spaceKey,
        [McpParameter("Content type: page, blogpost, or all (default: page)", required: false)] string contentType = "page",
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25,
        [McpParameter("Start index for pagination (default: 0)", required: false)] int start = 0,
        [McpParameter("Depth: root (top-level only) or all (default: all)", required: false)] string depth = "all")
    {
        try
        {
            var endpoint = $"space/{spaceKey}/content/{contentType}?limit={limit}&start={start}&depth={depth}&expand=version";
            var response = await ConfluenceClient.GetAsync(endpoint);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting space content: {ex.Message}";
        }
    }

    #endregion

    #region Search Operations

    [McpTool("confluence_search", "Search Confluence using CQL (Confluence Query Language)")]
    public static async Task<string> Search(
        [McpParameter("CQL query string (e.g., 'type=page AND space=DEV AND text~\"search term\"')")] string cql,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25,
        [McpParameter("Start index for pagination (default: 0)", required: false)] int start = 0,
        [McpParameter("Include content excerpt in results (default: true)", required: false)] bool includeExcerpt = true)
    {
        try
        {
            var encodedCql = HttpUtility.UrlEncode(cql);
            var expand = includeExcerpt ? "content.space,content.version,excerpt" : "content.space,content.version";
            var response = await ConfluenceClient.GetAsync($"search?cql={encodedCql}&limit={limit}&start={start}&expand={expand}");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error searching: {ex.Message}";
        }
    }

    [McpTool("confluence_search_content", "Simple text search across Confluence content")]
    public static async Task<string> SearchContent(
        [McpParameter("Search text")] string searchText,
        [McpParameter("Space key to limit search (optional)", required: false)] string? spaceKey = null,
        [McpParameter("Content type: page, blogpost, attachment, or all (default: all)", required: false)] string contentType = "all",
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var cqlParts = new List<string> { $"text~\"{searchText}\"" };

            if (!string.IsNullOrWhiteSpace(spaceKey))
                cqlParts.Add($"space={spaceKey}");

            if (contentType != "all")
                cqlParts.Add($"type={contentType}");

            var cql = string.Join(" AND ", cqlParts);
            var encodedCql = HttpUtility.UrlEncode(cql);

            var response = await ConfluenceClient.GetAsync($"search?cql={encodedCql}&limit={limit}&expand=content.space,excerpt");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error searching content: {ex.Message}";
        }
    }

    [McpTool("confluence_search_by_label", "Search Confluence content by label")]
    public static async Task<string> SearchByLabel(
        [McpParameter("Label name to search for")] string label,
        [McpParameter("Space key to limit search (optional)", required: false)] string? spaceKey = null,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var cqlParts = new List<string> { $"label=\"{label}\"" };

            if (!string.IsNullOrWhiteSpace(spaceKey))
                cqlParts.Add($"space={spaceKey}");

            var cql = string.Join(" AND ", cqlParts);
            var encodedCql = HttpUtility.UrlEncode(cql);

            var response = await ConfluenceClient.GetAsync($"search?cql={encodedCql}&limit={limit}&expand=content.space,content.metadata.labels");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error searching by label: {ex.Message}";
        }
    }

    #endregion

    #region Comment Operations

    [McpTool("confluence_get_page_comments", "Get comments on a Confluence page")]
    public static async Task<string> GetPageComments(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Comment depth: root (top-level) or all (default: all)", required: false)] string depth = "all",
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/child/comment?depth={depth}&limit={limit}&expand=body.storage,version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting comments: {ex.Message}";
        }
    }

    [McpTool("confluence_add_comment", "Add a comment to a Confluence page")]
    public static async Task<string> AddComment(
        [McpParameter("The page ID to comment on")] string pageId,
        [McpParameter("Comment text (in storage format or plain text)")] string commentText,
        [McpParameter("Parent comment ID for replies (optional)", required: false)] string? parentCommentId = null)
    {
        try
        {
            var commentData = new Dictionary<string, object>
            {
                ["type"] = "comment",
                ["container"] = new { id = pageId, type = "page" },
                ["body"] = new
                {
                    storage = new
                    {
                        value = $"<p>{HttpUtility.HtmlEncode(commentText)}</p>",
                        representation = "storage"
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(parentCommentId))
            {
                commentData["ancestors"] = new[] { new { id = parentCommentId } };
            }

            var response = await ConfluenceClient.PostAsync("content", commentData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error adding comment: {ex.Message}";
        }
    }

    [McpTool("confluence_delete_comment", "Delete a comment from a Confluence page")]
    public static async Task<string> DeleteComment(
        [McpParameter("The comment ID to delete")] string commentId)
    {
        try
        {
            await ConfluenceClient.DeleteAsync($"content/{commentId}");
            return $"Comment {commentId} deleted successfully";
        }
        catch (Exception ex)
        {
            return $"Error deleting comment: {ex.Message}";
        }
    }

    #endregion

    #region Attachment Operations

    [McpTool("confluence_get_attachments", "Get attachments on a Confluence page")]
    public static async Task<string> GetAttachments(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/child/attachment?limit={limit}&expand=version,metadata.mediaType");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting attachments: {ex.Message}";
        }
    }

    [McpTool("confluence_get_attachment_info", "Get information about a specific attachment")]
    public static async Task<string> GetAttachmentInfo(
        [McpParameter("The attachment ID")] string attachmentId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{attachmentId}?expand=version,container,metadata.mediaType");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting attachment info: {ex.Message}";
        }
    }

    [McpTool("confluence_delete_attachment", "Delete an attachment from a Confluence page")]
    public static async Task<string> DeleteAttachment(
        [McpParameter("The attachment ID to delete")] string attachmentId)
    {
        try
        {
            await ConfluenceClient.DeleteAsync($"content/{attachmentId}");
            return $"Attachment {attachmentId} deleted successfully";
        }
        catch (Exception ex)
        {
            return $"Error deleting attachment: {ex.Message}";
        }
    }

    #endregion

    #region Label Operations

    [McpTool("confluence_get_labels", "Get labels on a Confluence page")]
    public static async Task<string> GetLabels(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/label");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting labels: {ex.Message}";
        }
    }

    [McpTool("confluence_add_labels", "Add labels to a Confluence page")]
    public static async Task<string> AddLabels(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Comma-separated label names to add")] string labels)
    {
        try
        {
            var labelArray = labels.Split(',')
                .Select(l => new { prefix = "global", name = l.Trim().ToLowerInvariant() })
                .ToArray();

            var response = await ConfluenceClient.PostAsync($"content/{pageId}/label", labelArray);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error adding labels: {ex.Message}";
        }
    }

    [McpTool("confluence_remove_label", "Remove a label from a Confluence page")]
    public static async Task<string> RemoveLabel(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Label name to remove")] string label)
    {
        try
        {
            await ConfluenceClient.DeleteAsync($"content/{pageId}/label/{label.ToLowerInvariant()}");
            return $"Label '{label}' removed from page {pageId}";
        }
        catch (Exception ex)
        {
            return $"Error removing label: {ex.Message}";
        }
    }

    #endregion

    #region User Operations

    [McpTool("confluence_get_current_user", "Get information about the currently authenticated user")]
    public static async Task<string> GetCurrentUser()
    {
        try
        {
            var response = await ConfluenceClient.GetAsync("user/current");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting current user: {ex.Message}";
        }
    }

    [McpTool("confluence_search_users", "Search for Confluence users")]
    public static async Task<string> SearchUsers(
        [McpParameter("Search query (name or email)")] string query,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            // Use CQL to search for users
            var cql = $"user.fullname~\"{query}\" OR user.email~\"{query}\"";
            var encodedCql = HttpUtility.UrlEncode(cql);
            var response = await ConfluenceClient.GetAsync($"search/user?cql={encodedCql}&limit={limit}");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error searching users: {ex.Message}";
        }
    }

    [McpTool("confluence_get_user_content", "Get content created or contributed by a user")]
    public static async Task<string> GetUserContent(
        [McpParameter("Username or account ID")] string userKey,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var cql = $"creator=\"{userKey}\" ORDER BY lastmodified DESC";
            var encodedCql = HttpUtility.UrlEncode(cql);
            var response = await ConfluenceClient.GetAsync($"search?cql={encodedCql}&limit={limit}&expand=content.space");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting user content: {ex.Message}";
        }
    }

    #endregion

    #region Blog Post Operations

    [McpTool("confluence_create_blog_post", "Create a new blog post")]
    public static async Task<string> CreateBlogPost(
        [McpParameter("The space key")] string spaceKey,
        [McpParameter("The blog post title")] string title,
        [McpParameter("The blog post content in storage format")] string content)
    {
        try
        {
            var blogData = new Dictionary<string, object>
            {
                ["type"] = "blogpost",
                ["title"] = title,
                ["space"] = new { key = spaceKey },
                ["body"] = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                }
            };

            var response = await ConfluenceClient.PostAsync("content", blogData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error creating blog post: {ex.Message}";
        }
    }

    [McpTool("confluence_get_blog_posts", "Get blog posts from a space")]
    public static async Task<string> GetBlogPosts(
        [McpParameter("The space key")] string spaceKey,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}/content/blogpost?limit={limit}&expand=version,body.storage");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting blog posts: {ex.Message}";
        }
    }

    #endregion

    #region Template Operations

    [McpTool("confluence_get_templates", "Get available content templates")]
    public static async Task<string> GetTemplates(
        [McpParameter("Space key (optional - if not provided, returns global templates)", required: false)] string? spaceKey = null)
    {
        try
        {
            var endpoint = string.IsNullOrWhiteSpace(spaceKey)
                ? "template/page?expand=body"
                : $"template/page?spaceKey={spaceKey}&expand=body";

            var response = await ConfluenceClient.GetAsync(endpoint);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting templates: {ex.Message}";
        }
    }

    [McpTool("confluence_create_page_from_template", "Create a page from a template")]
    public static async Task<string> CreatePageFromTemplate(
        [McpParameter("The space key")] string spaceKey,
        [McpParameter("The page title")] string title,
        [McpParameter("Template ID")] string templateId,
        [McpParameter("Parent page ID (optional)", required: false)] string? parentId = null)
    {
        try
        {
            // First get the template content
            var templateResponse = await ConfluenceClient.GetAsync($"template/{templateId}?expand=body");
            var templateJson = JsonNode.Parse(templateResponse);
            var templateContent = templateJson?["body"]?["storage"]?["value"]?.GetValue<string>() ?? "";

            // Create page with template content
            var pageData = new Dictionary<string, object>
            {
                ["type"] = "page",
                ["title"] = title,
                ["space"] = new { key = spaceKey },
                ["body"] = new
                {
                    storage = new
                    {
                        value = templateContent,
                        representation = "storage"
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(parentId))
            {
                pageData["ancestors"] = new[] { new { id = parentId } };
            }

            var response = await ConfluenceClient.PostAsync("content", pageData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error creating page from template: {ex.Message}";
        }
    }

    #endregion

    #region Permissions and Restrictions

    [McpTool("confluence_get_page_restrictions", "Get restrictions (permissions) on a page")]
    public static async Task<string> GetPageRestrictions(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/restriction");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting page restrictions: {ex.Message}";
        }
    }

    [McpTool("confluence_get_space_permissions", "Get permissions for a space")]
    public static async Task<string> GetSpacePermissions(
        [McpParameter("The space key")] string spaceKey)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"space/{spaceKey}?expand=permissions");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting space permissions: {ex.Message}";
        }
    }

    #endregion

    #region Watch/Follow Operations

    [McpTool("confluence_watch_page", "Watch a page to receive notifications")]
    public static async Task<string> WatchPage(
        [McpParameter("The page ID to watch")] string pageId)
    {
        try
        {
            var currentUser = await ConfluenceClient.GetAsync("user/current");
            var userJson = JsonNode.Parse(currentUser);
            var username = userJson?["username"]?.GetValue<string>() ?? "";

            await ConfluenceClient.PostAsync($"user/watch/content/{pageId}", new { });
            return $"Now watching page {pageId}";
        }
        catch (Exception ex)
        {
            return $"Error watching page: {ex.Message}";
        }
    }

    [McpTool("confluence_unwatch_page", "Stop watching a page")]
    public static async Task<string> UnwatchPage(
        [McpParameter("The page ID to unwatch")] string pageId)
    {
        try
        {
            await ConfluenceClient.DeleteAsync($"user/watch/content/{pageId}");
            return $"Stopped watching page {pageId}";
        }
        catch (Exception ex)
        {
            return $"Error unwatching page: {ex.Message}";
        }
    }

    [McpTool("confluence_watch_space", "Watch a space to receive notifications")]
    public static async Task<string> WatchSpace(
        [McpParameter("The space key to watch")] string spaceKey)
    {
        try
        {
            await ConfluenceClient.PostAsync($"user/watch/space/{spaceKey}", new { });
            return $"Now watching space {spaceKey}";
        }
        catch (Exception ex)
        {
            return $"Error watching space: {ex.Message}";
        }
    }

    #endregion

    #region Content Properties

    [McpTool("confluence_get_content_properties", "Get custom properties on a page")]
    public static async Task<string> GetContentProperties(
        [McpParameter("The page ID")] string pageId)
    {
        try
        {
            var response = await ConfluenceClient.GetAsync($"content/{pageId}/property");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting content properties: {ex.Message}";
        }
    }

    [McpTool("confluence_set_content_property", "Set a custom property on a page")]
    public static async Task<string> SetContentProperty(
        [McpParameter("The page ID")] string pageId,
        [McpParameter("Property key")] string key,
        [McpParameter("Property value (JSON string)")] string value)
    {
        try
        {
            var propertyData = new
            {
                key,
                value = JsonNode.Parse(value)
            };

            var response = await ConfluenceClient.PostAsync($"content/{pageId}/property", propertyData);
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error setting content property: {ex.Message}";
        }
    }

    #endregion

    #region Recently Viewed/Modified

    [McpTool("confluence_get_recent_content", "Get recently modified content")]
    public static async Task<string> GetRecentContent(
        [McpParameter("Space key to filter (optional)", required: false)] string? spaceKey = null,
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var cqlParts = new List<string> { "type=page" };

            if (!string.IsNullOrWhiteSpace(spaceKey))
                cqlParts.Add($"space={spaceKey}");

            var cql = string.Join(" AND ", cqlParts) + " ORDER BY lastmodified DESC";
            var encodedCql = HttpUtility.UrlEncode(cql);

            var response = await ConfluenceClient.GetAsync($"search?cql={encodedCql}&limit={limit}&expand=content.space,content.version");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting recent content: {ex.Message}";
        }
    }

    [McpTool("confluence_get_my_recent_work", "Get content recently modified by current user")]
    public static async Task<string> GetMyRecentWork(
        [McpParameter("Maximum results (default: 25)", required: false)] int limit = 25)
    {
        try
        {
            var cql = "contributor=currentUser() ORDER BY lastmodified DESC";
            var encodedCql = HttpUtility.UrlEncode(cql);

            var response = await ConfluenceClient.GetAsync($"search?cql={encodedCql}&limit={limit}&expand=content.space");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting recent work: {ex.Message}";
        }
    }

    #endregion

    #region Server Info

    [McpTool("confluence_get_server_info", "Get Confluence server information")]
    public static async Task<string> GetServerInfo()
    {
        try
        {
            var response = await ConfluenceClient.GetAsync("settings/lookandfeel");
            return ConfluenceClient.FormatJsonResponse(response);
        }
        catch (Exception ex)
        {
            return $"Error getting server info: {ex.Message}";
        }
    }

    #endregion
}
