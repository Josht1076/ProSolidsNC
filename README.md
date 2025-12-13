# NCnetic Standalone — G-code backplot/simulation application

NCnetic Standalone is a Windows Forms desktop application that loads G-code files and visualizes the toolpath in 3D using the NCnetic core and view libraries. It is distributed as a self-contained app (not a Notepad++ plugin) so you can open, scrub through, and replay programs without installing additional editors.

![NCnetic Standalone preview](notepad_gcode.png)

## Features
- Load `.nc`, `.tap`, and other CNC program files and see the corresponding toolpath in a 3D viewport.
- Scrub through individual moves with a timeline control to inspect specific lines.
- Play/pause animation of the program to review motion sequences.
- Adjustable camera and view options powered by OpenTK.

## Building
The solution targets .NET 8 Windows desktop. You need the .NET 8 SDK with Windows targeting enabled.

1. Restore and build the solution:
   ```bash
   dotnet build NCneticStandalone.sln -c Release
   ```
2. The executable and supporting assemblies are produced under `NCneticStandalone/bin/Release/net8.0-windows7.0/`.

## Running
Run the built executable on Windows:
```bash
NCneticStandalone/bin/Release/net8.0-windows7.0/NCneticStandalone.exe
```

## Usage
1. Launch the application and open a G-code file (File → Open).
2. The editor panel shows the raw program while the right panel renders the toolpath.
3. Use the play/pause buttons to animate execution or drag the slider to jump to a specific move. Selecting a move highlights the corresponding line in the editor.

## Potential enhancements
- **Machine profiles and work offsets**: allow saving multiple machine configurations (travel limits, tool tables, G54–G59 offsets) and apply them when simulating a program.
- **Collision/limit warnings**: flag rapid moves outside machine limits or potential collisions with stock/fixtures before running on a machine.
- **Stock and tool library**: define stock geometry and cutting tools so rendered material removal better matches real jobs.
- **Measurement tools**: measure distances/angles in the 3D view to confirm clearances and feature sizes.
- **Step-through execution**: add single-line/step buttons to trace program flow line by line with call stack for subprograms.
- **Annotation and markup**: drop pins or comments on the toolpath to communicate issues or setup notes with operators.
- **File watching and reload**: auto-reload the visualization when the opened G-code file changes on disk during edits.
- **Import/export options**: export toolpaths as images/videos and import from additional formats (e.g., Heidenhain, Fanuc conversational where feasible).
- **Macro and param inspection**: surface variable values (e.g., `#100`) and macro evaluations to debug parameterized programs.

## Credits
- [OpenTK](https://github.com/opentk) for 3D graphics.
- NCnetic core and view components contained in this repository.
- Original project authors and contributors.
