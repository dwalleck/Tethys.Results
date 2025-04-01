#!/bin/bash
# Shell script to remove BOM characters from files
# Usage: ./remove-bom.sh -f path/to/file.cs or ./remove-bom.sh -d path/to/directory -e .cs

# Function to display usage
show_usage() {
  echo "Usage:"
  echo "  ./remove-bom.sh -f path/to/file.cs"
  echo "  ./remove-bom.sh -d path/to/directory -e .cs"
}

# Function to remove BOM from a file
remove_bom_from_file() {
  local file="$1"
  
  # Check if file has BOM
  if [ "$(hexdump -n 3 -e '3/1 "%02x"' "$file")" = "efbbbf" ]; then
    echo "Removing BOM from $file"
    
    # Create a temporary file
    local temp_file=$(mktemp)
    
    # Skip the first 3 bytes (BOM) and copy the rest to the temp file
    dd if="$file" of="$temp_file" bs=1 skip=3 status=none
    
    # Replace the original file with the temp file
    mv "$temp_file" "$file"
    
    echo "BOM removed successfully from $file"
  else
    echo "No BOM found in $file"
  fi
}

# Parse command line arguments
while getopts ":f:d:e:" opt; do
  case $opt in
    f)
      file="$OPTARG"
      ;;
    d)
      directory="$OPTARG"
      ;;
    e)
      extension="$OPTARG"
      ;;
    \?)
      echo "Invalid option: -$OPTARG" >&2
      show_usage
      exit 1
      ;;
    :)
      echo "Option -$OPTARG requires an argument." >&2
      show_usage
      exit 1
      ;;
  esac
done

# Process a single file
if [ -n "$file" ]; then
  if [ -f "$file" ]; then
    remove_bom_from_file "$file"
  else
    echo "File not found: $file" >&2
    exit 1
  fi
fi

# Process all files in a directory with the specified extension
if [ -n "$directory" ] && [ -n "$extension" ]; then
  if [ -d "$directory" ]; then
    # Find all files with the specified extension
    find "$directory" -type f -name "*$extension" | while read -r file; do
      remove_bom_from_file "$file"
    done
  else
    echo "Directory not found: $directory" >&2
    exit 1
  fi
fi

# If neither file nor directory is provided, show usage
if [ -z "$file" ] && [ -z "$directory" ]; then
  show_usage
  exit 1
fi
