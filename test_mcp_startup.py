#!/usr/bin/env python3
"""
Simple test to verify MCP server can start
"""

import asyncio
import os
import sys
import subprocess

def test_mcp_server():
    """Test if MCP server can start without errors"""
    
    # Set environment variables
    env = os.environ.copy()
    env['MSSQL_SERVER'] = 'heccdbs.database.windows.net,1433'
    env['MSSQL_DATABASE'] = 'TestApps'
    env['MSSQL_USER'] = 'hecc_admin'
    env['MSSQL_PASSWORD'] = 'czoYZEyiD24g5NT3h9L!PSy&y'
    
    python_path = r"C:\Users\hollo\AppData\Local\Programs\Python\Python312\python.exe"
    script_path = "sql_mcp_server.py"
    
    print("Testing MCP server startup...")
    print(f"Python: {python_path}")
    print(f"Script: {script_path}")
    print("Environment variables set")
    
    try:
        # Start the process and wait a bit to see if it starts without immediate errors
        process = subprocess.Popen(
            [python_path, script_path],
            env=env,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )
        
        # Wait a short time to see if it starts successfully
        try:
            stdout, stderr = process.communicate(timeout=3)
            print(f"Process exited with code: {process.returncode}")
            if stdout:
                print(f"STDOUT: {stdout}")
            if stderr:
                print(f"STDERR: {stderr}")
        except subprocess.TimeoutExpired:
            print("MCP server started successfully (running in background)")
            process.terminate()
            try:
                process.wait(timeout=1)
                print("Process terminated gracefully")
            except subprocess.TimeoutExpired:
                process.kill()
                print("Process killed")
        
    except Exception as e:
        print(f"Error starting MCP server: {e}")

if __name__ == "__main__":
    test_mcp_server()
