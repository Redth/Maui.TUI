#!/bin/bash
# Launch the Maui.TUI sample app in a new terminal window
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SAMPLE_DIR="$SCRIPT_DIR/samples/Maui.TUI.Sample"

osascript -e "
tell application \"Terminal\"
    activate
    do script \"cd '$SAMPLE_DIR' && dotnet run\"
end tell
"
