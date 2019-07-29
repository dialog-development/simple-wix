# Getting Started

If you want to create an msi installer that just copies files from your developer machine, into a specific destination on the user's machine, then Simple WiX is a simple to use, open source option, that is also designed to integrate cleanly into a continuous integration / continuous delivery workflow. 

Let's start with an example; personally I write a lot of plugins for Autodesk's Revit, a 3D modelling software. These plugins are written in C# and ultimately produce a bunch of mostly DLL files, that just need to be copied into the right Autodesk folder under a user's Roaming\AppData folder.

![alt text](../media/revit_addin_folder.png "Installation Directory")