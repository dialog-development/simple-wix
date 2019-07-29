# Getting Started

## A Little Background

Let's start with an example; personally I write a lot of plugins for Autodesk's Revit, a 3D modelling software. These plugins are written in C# and ultimately produce a bunch of mostly DLL files, something like this:

<p align="center">
<img src="../media/gettingstarted/source_pb.png" alt="Simple Wix" height="150"/>
</p>

To install this on a user's machine, I just just need either the individual files or the whole Photobooth folder to be copied into the right Autodesk folder under a user's Roaming\AppData folder.

<p align="center">
<img src="../media/gettingstarted/revit_addin_folder.png" alt="Simple Wix" height="300"/>
</p>
![alt text](../media/gettingstarted/revit_addin_folder.png "Installation Directory")

But when you go to acually distribute these files, a number of questions start coming up, like 'how do I get these files into the right spot on the user's machine', 'how do I update those files when new versions are released', 'what happens if it's interrupted',and 'how does the user remove it cleanly and easily'? 

Thankfully we're not the first people who needed to solve these problems, and the most reliable solution on a Windows machine is to integrate with the Windows Installer Framework by creating .msi files.

<p align="center">
<img src=../media/gettingstarted/installer.png alt="Simple Wix" height="100"/>
</p>

That being said, the Windows Installer Framework is quite low level, with a number of complicated steps and registries and tables to manage, so it's typically not used directly, but through intermediary frameworks. 

One of the most reliable and robust ones is the Microsoft published open source WiX framework. While it works reliably and is easy to debug, imho, it's a little too low level for many simple development cases. That's why I created the Simple-Wix utility to create your WiX files for you. That way you get all the benefits of WiX, without the hastle of actually writing WiX. 

<p align="center">
<img src=../media/gettingstarted/process_simple.png alt="Simple Wix" height="100"/>
</p>

## Quick Start

To package my files up for installation, I just need to follow these steps:

1. Download and install the [WiX Toolset](https://wixtoolset.org/releases/).

2. Download the SimpleWix.exe file somewhere on your computer, I'm going to put it right next to my installation folder to make writing relative paths a little easier:
<p align="center">
<img src=../media/gettingstarted/simplewix_copied.png alt="Simple Wix" height=150/>
</p>

3. Generate a manifest file for your project, I'm going to use the command line and call *simplewix.exe init* to walk me through this:
<p align="center">
<img src=../media/gettingstarted/simplewix_init.png alt="Simple Wix" height="350"/>
</p>
<p align="center">
<img src=../media/gettingstarted/manifest_created.png alt="Simple Wix" 
height=150/>
</p>

4. Now that you have a manifest, you can simply run simplewix.exe from the the command line with no arguments, or double click on it in file explorer and it will generate several intermediary files for debugging along with your completed .msi file!

<p align="center">
<img src=../media/gettingstarted/simplewix_complete.png alt="Simple Wix"  height="600"/>
</p>

<p align="center">
<img src=../media/gettingstarted/simplewix_msi.png alt="Simple Wix" height="200"/>
</p>

5. And that's it, simply run your msi file, click "next, next, next" like always, and those files will be installed on your machine with proper registration for clean upgrades and uninstallations. 

<p align="center">
<img src=../media/gettingstarted/installer_welcome.png alt="Simple Wix" height="300"  />
</p>

<p align="center">
<img src=../media/gettingstarted/installer_license.png alt="Simple Wix" height="300" />
</p>

<p align="center">
<img src=../media/gettingstarted/installer_features.png alt="Simple Wix" height="300" />
</p>


<p align="center">
<img src=../media/gettingstarted/installer_final.png alt="Simple Wix"  height=300/>
</p>

<p align="center">
<img src=../media/gettingstarted/installer_programlist.png alt="Simple Wix" height=400/>
</p>

If you'd like some more detail on this process and the various options available to customize your installation (like how to change the default images) you can find it [here](moredetail.md).

<i> Final Note:</i> Hang onto your manifest file, and specifically the GUIDs created by both the UpgradeCode and the ProductCode. These two GUIDs are what uniquely identify your product to Windows so that it knows when to upgrade vs create a new installation. If these change Windows will see your two versions as completely unrelated applications. 

