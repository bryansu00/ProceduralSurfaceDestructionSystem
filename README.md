# Procedural Surface Destruction System
## About
The Procedural Surface Destruction System is essentially a real-time 3D mesh generator that is meant to be used in video games.
This project was inspired by the video game "Rainbow Six: Siege" (R6S), and the implemenation of the project is based
on a GDC Talk called ["The Art of Destruction in Rainbow Six: Siege"](https://www.youtube.com/watch?v=SjkQxowsL0I&t).

This project used Raylib-CS to test algorithms in 2D first, and then once the 2D portion of the project is figured out, the algorithms was implemented in the Godot Game Engine.

## Usage
Both of the 2D implementation and Godot implementation of this project is dependent on the .dll file compiled using the PSDSystemLibrary project. This repository already has the .dll file compiled for those two projects. However, if the code for the PSDSystemLibrary needs to be edited and then recompiled, the PSDSystemLibrary folder contains the source code for doing that.

To compile, open PSDSystemLibrary/PSDSystemLibrary.sln, set the solution configuration from "Debug" to "Release", and then build the project using the Ctrl+B shortcut or the "Build PSDSystemLibrary" button under the "Build" tab. Once built, the .dll file should be located in the PSDSystemLibrary/PSDSystemLibrary/bin/Release/net6.0 folder. An .xml file is also included to enable the use of intellisense in VisualStudio.

To run the 2D implementation of this project, open 2D-Implementation/2D-Implementation.sln using Visual Studio 2022 and then simply hit the Run button.

To open the Godot implementation of this project, download Godot Engine (.Net version) from [godotengine.org/](https://godotengine.org/). Then use Godot to open 
Godot Projects/game1/project.godot, and then you should be able to run a 3D demo of the project.
