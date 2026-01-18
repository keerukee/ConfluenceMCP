using MCPServer;

// Confluence MCP Server - Supports both Confluence Cloud and Datacenter
// 
// Environment Variables Required:
// - CONFLUENCE_BASE_URL: Your Confluence instance URL (e.g., https://your-domain.atlassian.net/wiki)
// - CONFLUENCE_DEPLOYMENT_TYPE: "cloud" or "datacenter"
// - CONFLUENCE_API_TOKEN: API token (Cloud) or PAT token (Datacenter)
// - CONFLUENCE_EMAIL: Email address (required for Cloud only)

var server = new McpServerHost(new McpServerOptions
{
    ServerName = "confluence-mcp-server",
    ServerVersion = "1.0.0"
});

server.Run();


