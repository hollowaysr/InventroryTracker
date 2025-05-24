#!/usr/bin/env python3
"""
Minimal MCP Server test
"""

from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp import types
import asyncio

# Create a minimal server
server = Server("test-server")

@server.list_tools()
async def list_tools():
    return [
        types.Tool(
            name="test_tool",
            description="A simple test tool",
            inputSchema={
                "type": "object",
                "properties": {
                    "message": {"type": "string"}
                },
                "required": ["message"]
            }
        )
    ]

@server.call_tool()
async def call_tool(name: str, arguments: dict):
    if name == "test_tool":
        return [types.TextContent(
            type="text", 
            text=f"Hello from test tool! Message: {arguments.get('message', 'No message')}"
        )]

async def main():
    async with stdio_server() as (read_stream, write_stream):
        await server.run(read_stream, write_stream, server.create_initialization_options())

if __name__ == "__main__":
    print("Starting minimal MCP server...")
    asyncio.run(main())
