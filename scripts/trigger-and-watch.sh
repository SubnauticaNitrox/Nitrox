#!/bin/bash
set -e

WORKFLOW_FILE="build-macos.yml"

# Check if logged in
if ! gh auth status &>/dev/null; then
    echo "Error: You are not logged in to GitHub CLI."
    echo "Please run 'gh auth login' first."
    exit 1
fi

echo "Triggering workflow '$WORKFLOW_FILE' on master..."
gh workflow run "$WORKFLOW_FILE" --ref master

echo "Waiting for workflow run to start..."
sleep 5

# Get the ID of the most recent run for this workflow
# We filter by the current branch to be safe
RUN_ID=$(gh run list --workflow="$WORKFLOW_FILE" --branch master --limit 1 --json databaseId --jq '.[0].databaseId')

if [ -z "$RUN_ID" ]; then
    echo "Error: Could not find the workflow run."
    exit 1
fi

echo "Found Run ID: $RUN_ID"
echo "Watching build progress... (Press Ctrl+C to stop watching, build will continue)"
echo "View on web: https://github.com/SnowballXueQiu/Nitrox/actions/runs/$RUN_ID"

# Watch the run interactively
gh run watch "$RUN_ID"

# Check final status
CONCLUSION=$(gh run view "$RUN_ID" --json conclusion --jq '.conclusion')

if [ "$CONCLUSION" == "success" ]; then
    echo "✅ Build succeeded!"
    
    # Ask to download artifact
    read -p "Do you want to download the installer artifact? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "Downloading artifact..."
        gh run download "$RUN_ID" --name "Nitrox-Installer"
        echo "Downloaded to current directory."
    fi
else
    echo "❌ Build failed with status: $CONCLUSION"
    echo "Fetching logs..."
    gh run view "$RUN_ID" --log
fi
