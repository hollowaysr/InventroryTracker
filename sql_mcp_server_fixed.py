#!/usr/bin/env python3
"""
SQL Server MCP Server for VS Code
A Model Context Protocol server for Microsoft SQL Server integration
"""

import asyncio
import logging
import os
from typing import Any, Dict, List
import pyodbc
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp import types

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Create server instance
server = Server("sql-server-mcp")

def get_connection_string():
    """Get SQL Server connection string from environment variables"""
    server_addr = os.getenv('MSSQL_SERVER', 'heccdbs.database.windows.net,1433')
    database = os.getenv('MSSQL_DATABASE', 'TestApps')
    username = os.getenv('MSSQL_USER', 'hecc_admin')
    password = os.getenv('MSSQL_PASSWORD', '')
    
    if not password:
        logger.warning("No password provided. Please set MSSQL_PASSWORD environment variable.")
    
    # Build connection string for pyodbc
    connection_string = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={server_addr};"
        f"DATABASE={database};"
        f"UID={username};"
        f"PWD={password};"
        f"Encrypt=yes;"
        f"TrustServerCertificate=no;"
        f"Connection Timeout=30;"
    )
    
    return connection_string

async def execute_sql_query(query: str) -> List[Dict[str, Any]]:
    """Execute SQL query and return results"""
    connection_string = get_connection_string()
    
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        
        cursor.execute(query)
        
        # Check if query returns results
        if cursor.description:
            columns = [column[0] for column in cursor.description]
            rows = cursor.fetchall()
            results = []
            
            for row in rows:
                result = {}
                for i, value in enumerate(row):
                    result[columns[i]] = str(value) if value is not None else None
                results.append(result)
        else:
            # For non-SELECT queries (INSERT, UPDATE, DELETE, etc.)
            affected_rows = cursor.rowcount
            results = [{"affected_rows": affected_rows, "message": f"Query executed successfully. {affected_rows} rows affected."}]
        
        cursor.close()
        conn.close()
        
        return results
        
    except Exception as e:
        logger.error(f"SQL execution error: {str(e)}")
        raise

@server.list_tools()
async def handle_list_tools() -> List[types.Tool]:
    """List available tools"""
    return [
        types.Tool(
            name="execute_sql",
            description="Execute SQL query against the connected SQL Server database",
            inputSchema={
                "type": "object",
                "properties": {
                    "query": {
                        "type": "string",
                        "description": "SQL query to execute"
                    }
                },
                "required": ["query"]
            }
        ),
        types.Tool(
            name="list_tables",
            description="List all tables in the database",
            inputSchema={
                "type": "object",
                "properties": {}
            }
        ),
        types.Tool(
            name="describe_table",
            description="Get schema information for a specific table",
            inputSchema={
                "type": "object",
                "properties": {
                    "table_name": {
                        "type": "string",
                        "description": "Name of the table to describe"
                    }
                },
                "required": ["table_name"]
            }
        )
    ]

@server.call_tool()
async def handle_call_tool(name: str, arguments: Dict[str, Any]) -> List[types.TextContent]:
    """Handle tool calls"""
    try:
        if name == "execute_sql":
            query = arguments.get("query", "")
            if not query.strip():
                return [types.TextContent(
                    type="text",
                    text="Error: No SQL query provided"
                )]
            
            results = await execute_sql_query(query)
            
            if not results:
                response_text = "Query executed successfully. No results returned."
            else:
                response_text = f"Query executed successfully. Found {len(results)} result(s):\n\n"
                for i, result in enumerate(results[:10]):  # Limit to first 10 results
                    response_text += f"Row {i+1}: {result}\n"
                
                if len(results) > 10:
                    response_text += f"\n... and {len(results) - 10} more rows"
            
            return [types.TextContent(type="text", text=response_text)]
            
        elif name == "list_tables":
            query = """
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME
            """
            results = await execute_sql_query(query)
            
            if not results:
                response_text = "No tables found in the database."
            else:
                table_names = [result["TABLE_NAME"] for result in results]
                response_text = f"Found {len(table_names)} tables in the database:\n\n"
                response_text += "\n".join([f"- {table}" for table in table_names])
            
            return [types.TextContent(type="text", text=response_text)]
            
        elif name == "describe_table":
            table_name = arguments.get("table_name", "")
            if not table_name.strip():
                return [types.TextContent(
                    type="text",
                    text="Error: No table name provided"
                )]
            
            query = f"""
            SELECT 
                COLUMN_NAME,
                DATA_TYPE,
                IS_NULLABLE,
                COLUMN_DEFAULT,
                CHARACTER_MAXIMUM_LENGTH
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = '{table_name}'
            ORDER BY ORDINAL_POSITION
            """
            results = await execute_sql_query(query)
            
            if not results:
                response_text = f"Table '{table_name}' not found or has no columns."
            else:
                response_text = f"Schema for table '{table_name}':\n\n"
                response_text += "| Column | Type | Nullable | Default | Max Length |\n"
                response_text += "|--------|------|----------|---------|------------|\n"
                
                for col in results:
                    response_text += f"| {col['COLUMN_NAME']} | {col['DATA_TYPE']} | {col['IS_NULLABLE']} | {col['COLUMN_DEFAULT'] or 'NULL'} | {col['CHARACTER_MAXIMUM_LENGTH'] or 'N/A'} |\n"
            
            return [types.TextContent(type="text", text=response_text)]
            
        else:
            return [types.TextContent(
                type="text",
                text=f"Error: Unknown tool '{name}'"
            )]
            
    except Exception as e:
        logger.error(f"Tool execution error: {str(e)}")
        return [types.TextContent(
            type="text",
            text=f"Error executing tool '{name}': {str(e)}"
        )]

async def main():
    """Main entry point"""
    async with stdio_server() as (read_stream, write_stream):
        await server.run(
            read_stream,
            write_stream,
            server.create_initialization_options()
        )

if __name__ == "__main__":
    asyncio.run(main())
