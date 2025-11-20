#!/bin/bash
# Clean all obj and bin directories

echo "Cleaning AxisCore solution..."

# Find and remove all obj and bin directories
find . -type d \( -name "obj" -o -name "bin" \) -exec rm -rf {} + 2>/dev/null || true

echo "Restoring packages..."
dotnet restore

echo "Done! You can now build the solution."
