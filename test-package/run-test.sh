#!/bin/bash

echo "Running Tethys.Results test package..."
echo

# Restore packages
echo "Restoring packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "Failed to restore packages."
    exit 1
fi

# Build the project
echo "Building project..."
dotnet build
if [ $? -ne 0 ]; then
    echo "Failed to build project."
    exit 1
fi

# Run the test
echo "Running test..."
echo
dotnet run
if [ $? -ne 0 ]; then
    echo "Test failed with error code $?."
    exit 1
fi

echo
echo "Test completed successfully!"
read -p "Press Enter to continue..."
