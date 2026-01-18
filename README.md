# Confluence MCP Server

A Model Context Protocol (MCP) server for Confluence that enables AI assistants to interact with Confluence Cloud and Datacenter instances. Built with .NET 10 using the [Keerukee.MCPServer.Stdio](https://www.nuget.org/packages/Keerukee.MCPServer.Stdio) library.

## Features

- ✅ **Dual Deployment Support**: Works with both Confluence Cloud (API token + email) and Confluence Datacenter (PAT token)
- ✅ **Page Management**: Create, read, update, delete, copy, and move pages
- ✅ **Space Operations**: List, create, and manage spaces
- ✅ **Search**: Full CQL (Confluence Query Language) support
- ✅ **Comments**: Add and manage page comments
- ✅ **Attachments**: List and manage page attachments
- ✅ **Labels**: Add, remove, and search by labels
- ✅ **Blog Posts**: Create and manage blog posts
- ✅ **Templates**: Use and create pages from templates
- ✅ **Version History**: Access page history and restore versions
- ✅ **Permissions**: View page restrictions and space permissions
- ✅ **Watch/Follow**: Subscribe to page and space notifications

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Confluence Cloud or Datacenter instance
- API credentials (see Configuration section)

## Installation

### Build the Server

```bash
git clone <repository-url>
cd ConfluenceMcpServer
dotnet build -c Release
```

### Publish as Executable (Recommended)

**Cross-platform (requires .NET runtime):**
```bash
dotnet publish -c Release -o ./publish
```

**Self-contained executable (no .NET runtime required):**
```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained -o ./publish

# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained -o ./publish

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained -o ./publish

# Linux
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish
```

After publishing, you'll have either:
- `ConfluenceMcpServer.exe` (Windows) or `ConfluenceMcpServer` (macOS/Linux) - standalone executable
- `ConfluenceMcpServer.dll` - requires `dotnet ConfluenceMcpServer.dll` to run

## Configuration

The server uses environment variables for configuration:

| Variable | Required | Description |
|----------|----------|-------------|
| `CONFLUENCE_BASE_URL` | Yes | Your Confluence instance URL |
| `CONFLUENCE_DEPLOYMENT_TYPE` | Yes | `cloud` or `datacenter` |
| `CONFLUENCE_API_TOKEN` | Yes | API token (Cloud) or PAT (Datacenter) |
| `CONFLUENCE_EMAIL` | Cloud only | Email associated with API token |

### Confluence Cloud Setup

1. Go to [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Click "Create API token"
3. Copy the token and set environment variables:

```bash
export CONFLUENCE_BASE_URL="https://your-domain.atlassian.net/wiki"
export CONFLUENCE_DEPLOYMENT_TYPE="cloud"
export CONFLUENCE_API_TOKEN="your-api-token"
export CONFLUENCE_EMAIL="your-email@example.com"
```

### Confluence Datacenter Setup

1. Go to your Confluence profile → Personal Access Tokens
2. Create a new token with appropriate permissions
3. Set environment variables:

```bash
export CONFLUENCE_BASE_URL="https://confluence.your-company.com"
export CONFLUENCE_DEPLOYMENT_TYPE="datacenter"
export CONFLUENCE_API_TOKEN="your-pat-token"
```

---

## Client Configuration

### Claude Desktop

Add to your Claude Desktop configuration file:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

#### Using Executable (Recommended)

```json
{
  "mcpServers": {
    "confluence": {
      "command": "/path/to/publish/ConfluenceMcpServer.exe",
      "env": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  }
}
```

#### Using DLL (requires .NET runtime)

```json
{
  "mcpServers": {
    "confluence": {
      "command": "dotnet",
      "args": ["/path/to/publish/ConfluenceMcpServer.dll"],
      "env": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  }
}
```

---

### Visual Studio GitHub Copilot

To use with GitHub Copilot in Visual Studio:

1. **Publish the server** as an executable:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained -o ./publish
   ```

2. **Configure MCP servers** in Visual Studio:
   - Go to **Tools** → **Options** → **GitHub Copilot** → **MCP Servers**
   - Or add to your `.vs/mcp.json` file in your solution:

#### Using Executable (Recommended)

```json
{
  "servers": {
    "confluence": {
      "command": "C:/path/to/publish/ConfluenceMcpServer.exe",
      "env": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  }
}
```

#### Using DLL (requires .NET runtime)

```json
{
  "servers": {
    "confluence": {
      "command": "dotnet",
      "args": ["C:/path/to/publish/ConfluenceMcpServer.dll"],
      "env": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  }
}
```

3. **Alternative**: Use user-level configuration at `%USERPROFILE%\.copilot\mcp.json`

---

### Windsurf (Codeium)

Add to your Windsurf MCP configuration:

**macOS**: `~/.codeium/windsurf/mcp_config.json`  
**Windows**: `%USERPROFILE%\.codeium\windsurf\mcp_config.json`

#### Using Executable (Recommended)

```json
{
  "mcpServers": {
    "confluence": {
      "command": "/path/to/publish/ConfluenceMcpServer.exe",
      "env": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  }
}
```

#### Using DLL (requires .NET runtime)

```json
{
  "mcpServers": {
    "confluence": {
      "command": "dotnet",
      "args": ["/path/to/publish/ConfluenceMcpServer.dll"],
      "env": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  }
}
```

#### Datacenter Example

```json
{
  "mcpServers": {
    "confluence": {
      "command": "/path/to/publish/ConfluenceMcpServer.exe",
      "env": {
        "CONFLUENCE_BASE_URL": "https://confluence.your-company.com",
        "CONFLUENCE_DEPLOYMENT_TYPE": "datacenter",
        "CONFLUENCE_API_TOKEN": "your-pat-token"
      }
    }
  }
}
```

---

### Anthropic (Antigravity / Claude API)

For custom Claude API integrations or the Antigravity platform:

1. **Publish the server** as an executable
2. **Start the server** as a subprocess with stdio transport
3. **Connect via MCP protocol** over stdin/stdout

#### Example Python Integration

```python
import subprocess
import json
import os

# Start the MCP server using executable
process = subprocess.Popen(
    ["/path/to/publish/ConfluenceMcpServer.exe"],  # or ConfluenceMcpServer on macOS/Linux
    stdin=subprocess.PIPE,
    stdout=subprocess.PIPE,
    env={
        **os.environ,
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
    }
)

# Or using DLL with dotnet runtime
process = subprocess.Popen(
    ["dotnet", "/path/to/publish/ConfluenceMcpServer.dll"],
    stdin=subprocess.PIPE,
    stdout=subprocess.PIPE,
    env={
        **os.environ,
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
    }
)

# Send MCP initialize request
request = {
    "jsonrpc": "2.0",
    "id": 1,
    "method": "initialize",
    "params": {
        "protocolVersion": "2024-11-05",
        "capabilities": {},
        "clientInfo": {"name": "my-client", "version": "1.0.0"}
    }
}
process.stdin.write(json.dumps(request).encode() + b'\n')
process.stdin.flush()
```

#### Antigravity Configuration

For **Antigravity** specifically, configure in your agent settings:

**Using Executable:**
```json
{
  "mcp_servers": [
    {
      "name": "confluence",
      "transport": "stdio",
      "command": "/path/to/publish/ConfluenceMcpServer.exe",
      "environment": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  ]
}
```

**Using DLL:**
```json
{
  "mcp_servers": [
    {
      "name": "confluence",
      "transport": "stdio",
      "command": "dotnet",
      "args": ["/path/to/publish/ConfluenceMcpServer.dll"],
      "environment": {
        "CONFLUENCE_BASE_URL": "https://your-domain.atlassian.net/wiki",
        "CONFLUENCE_DEPLOYMENT_TYPE": "cloud",
        "CONFLUENCE_API_TOKEN": "your-api-token",
        "CONFLUENCE_EMAIL": "your-email@example.com"
      }
    }
  ]
}
```

---

## Available Tools

### Page Operations

| Tool | Description |
|------|-------------|
| `confluence_get_page` | Get a page by ID with optional body content |
| `confluence_get_page_by_title` | Get a page by title and space key |
| `confluence_create_page` | Create a new page |
| `confluence_update_page` | Update an existing page |
| `confluence_delete_page` | Delete a page (trash or purge) |
| `confluence_get_page_children` | Get child pages |
| `confluence_get_page_ancestors` | Get parent pages |
| `confluence_get_page_history` | Get version history |
| `confluence_get_page_version` | Get specific version |
| `confluence_copy_page` | Copy a page to new location |
| `confluence_move_page` | Move a page to new parent |

### Space Operations

| Tool | Description |
|------|-------------|
| `confluence_list_spaces` | List all accessible spaces |
| `confluence_get_space` | Get space details |
| `confluence_create_space` | Create a new space |
| `confluence_delete_space` | Delete a space |
| `confluence_get_space_content` | Get all content in a space |

### Search Operations

| Tool | Description |
|------|-------------|
| `confluence_search` | Search using CQL |
| `confluence_search_content` | Simple text search |
| `confluence_search_by_label` | Search by label |

### Comment Operations

| Tool | Description |
|------|-------------|
| `confluence_get_page_comments` | Get comments on a page |
| `confluence_add_comment` | Add a comment |
| `confluence_delete_comment` | Delete a comment |

### Attachment Operations

| Tool | Description |
|------|-------------|
| `confluence_get_attachments` | Get page attachments |
| `confluence_get_attachment_info` | Get attachment details |
| `confluence_delete_attachment` | Delete an attachment |

### Label Operations

| Tool | Description |
|------|-------------|
| `confluence_get_labels` | Get labels on a page |
| `confluence_add_labels` | Add labels to a page |
| `confluence_remove_label` | Remove a label |

### User Operations

| Tool | Description |
|------|-------------|
| `confluence_get_current_user` | Get authenticated user |
| `confluence_search_users` | Search for users |
| `confluence_get_user_content` | Get user's content |

### Blog Post Operations

| Tool | Description |
|------|-------------|
| `confluence_create_blog_post` | Create a blog post |
| `confluence_get_blog_posts` | Get blog posts in a space |

### Template Operations

| Tool | Description |
|------|-------------|
| `confluence_get_templates` | Get available templates |
| `confluence_create_page_from_template` | Create page from template |

### Permission Operations

| Tool | Description |
|------|-------------|
| `confluence_get_page_restrictions` | Get page restrictions |
| `confluence_get_space_permissions` | Get space permissions |

### Watch/Follow Operations

| Tool | Description |
|------|-------------|
| `confluence_watch_page` | Watch a page |
| `confluence_unwatch_page` | Stop watching a page |
| `confluence_watch_space` | Watch a space |

### Content Properties

| Tool | Description |
|------|-------------|
| `confluence_get_content_properties` | Get custom properties |
| `confluence_set_content_property` | Set a custom property |

### Recent Content

| Tool | Description |
|------|-------------|
| `confluence_get_recent_content` | Get recently modified content |
| `confluence_get_my_recent_work` | Get current user's recent work |

---

## Available Resources

### Configuration

| Resource URI | Description |
|--------------|-------------|
| `confluence://config` | Current configuration (sanitized) |

### Page Resources

| Resource URI | Description |
|--------------|-------------|
| `confluence://page/{pageId}` | Page details |
| `confluence://page/{pageId}/body` | Page body content (HTML) |
| `confluence://page/{pageId}/children` | Child pages |
| `confluence://page/{pageId}/comments` | Page comments |
| `confluence://page/{pageId}/attachments` | Page attachments |
| `confluence://page/{pageId}/labels` | Page labels |
| `confluence://page/{pageId}/history` | Page version history |

### Space Resources

| Resource URI | Description |
|--------------|-------------|
| `confluence://spaces` | All accessible spaces |
| `confluence://space/{spaceKey}` | Space details |
| `confluence://space/{spaceKey}/pages` | All pages in space |
| `confluence://space/{spaceKey}/blogposts` | Blog posts in space |
| `confluence://space/{spaceKey}/root-pages` | Top-level pages |
| `confluence://space/{spaceKey}/templates` | Space templates |

### User Resources

| Resource URI | Description |
|--------------|-------------|
| `confluence://current-user` | Authenticated user info |
| `confluence://my-recent-work` | User's recent activity |

### Search Resources

| Resource URI | Description |
|--------------|-------------|
| `confluence://search/{query}` | Search results |
| `confluence://label/{label}` | Content by label |

### Recent Content

| Resource URI | Description |
|--------------|-------------|
| `confluence://recent-pages` | Recently modified pages |
| `confluence://recent-blogposts` | Recent blog posts |
| `confluence://templates` | Global templates |

---

## Usage Examples

Once configured, you can ask your AI assistant:

### Page Management
- "Create a new page in the DEV space titled 'API Documentation'"
- "Get the content of page 123456"
- "Update the page titled 'Meeting Notes' in space HR"
- "Copy page 123456 to the ARCHIVE space"
- "Move page 123456 under page 789012"

### Search and Discovery
- "Search for pages containing 'deployment guide'"
- "Find all pages with label 'architecture'"
- "List all pages in the DEV space"
- "Show me recently modified pages"

### Comments and Collaboration
- "Add a comment to page 123456 saying 'Great documentation!'"
- "Get all comments on the API Documentation page"
- "Watch the Architecture space for updates"

### Space Management
- "List all spaces I have access to"
- "Get details about the DEV space"
- "Create a new space called 'Project Alpha' with key ALPHA"

### Labels and Organization
- "Add labels 'important, reviewed' to page 123456"
- "Find all pages labeled 'draft'"
- "Remove the 'outdated' label from page 123456"

### Templates
- "List available templates in the DEV space"
- "Create a new page from the 'Meeting Notes' template"

---

## CQL Query Examples

The `confluence_search` tool accepts CQL queries:

```
# Find pages in a specific space
type=page AND space=DEV

# Full-text search
text~"deployment guide"

# Pages modified recently
type=page AND lastmodified>=now("-7d")

# Pages with specific labels
label="architecture" AND label="approved"

# Pages created by specific user
creator=currentUser() AND type=page

# Blog posts from this year
type=blogpost AND created>=2024-01-01

# Combined complex query
type=page AND space IN (DEV, PROD) AND label="api" AND lastmodified>=now("-30d") ORDER BY lastmodified DESC
```

---

## Confluence Storage Format (XHTML)

When creating or updating pages, use Confluence storage format (XHTML):

```html
<h1>Heading</h1>
<p>This is a paragraph with <strong>bold</strong> and <em>italic</em> text.</p>

<ul>
  <li>Bullet point 1</li>
  <li>Bullet point 2</li>
</ul>

<ol>
  <li>Numbered item 1</li>
  <li>Numbered item 2</li>
</ol>

<ac:structured-macro ac:name="code">
  <ac:parameter ac:name="language">python</ac:parameter>
  <ac:plain-text-body><![CDATA[print("Hello, World!")]]></ac:plain-text-body>
</ac:structured-macro>

<ac:structured-macro ac:name="info">
  <ac:rich-text-body>
    <p>This is an info panel</p>
  </ac:rich-text-body>
</ac:structured-macro>
```

---

## Troubleshooting

### Common Issues

**"CONFLUENCE_BASE_URL environment variable is not set"**
- Ensure all required environment variables are configured
- For Cloud, include `/wiki` in the URL: `https://your-domain.atlassian.net/wiki`

**"401 Unauthorized"**
- Verify your API token/PAT is correct and not expired
- For Cloud: ensure email matches the Atlassian account
- For Datacenter: ensure PAT has required permissions

**"404 Not Found"**
- Check that the page/space ID or key is correct
- Verify you have permission to access the content

**"Connection refused"**
- Check CONFLUENCE_BASE_URL is accessible from your machine
- Verify no firewall/proxy blocking the connection

**Server not starting**
- Ensure .NET 10 SDK is installed: `dotnet --version`
- Try building first: `dotnet build`

### Debug Mode

To see detailed MCP communication:

```bash
# Run with output logging
dotnet run 2>&1 | tee mcp-debug.log
```

---

## Project Structure

```
ConfluenceMcpServer/
├── Confluence_MCP.csproj          # Project file with NuGet reference
├── Program.cs                      # Server entry point
├── README.md                       # This file
├── Configuration/
│   └── ConfluenceConfiguration.cs # Environment-based config
├── Services/
│   └── ConfluenceClient.cs        # HTTP client for Confluence API
├── Tools/
│   └── ConfluenceTools.cs         # MCP tool implementations (45+ tools)
└── Resources/
    └── ConfluenceResources.cs     # MCP resource implementations (20+ resources)
```

---

## License

MIT License - See LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Acknowledgments

- Built with [Keerukee.MCPServer.Stdio](https://www.nuget.org/packages/Keerukee.MCPServer.Stdio)
- Implements the [Model Context Protocol](https://modelcontextprotocol.io/)
