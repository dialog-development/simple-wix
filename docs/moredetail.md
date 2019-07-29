# More Detail


## A Quick Overview Of WiX
In a nutshell, this framework let's you use an XML file to describe how you want the files system on the user's machine to look, and will then grab all the source files and compile your .msi file. However, WiX files require you to describe every single file and folder with a unique name and ID and are quite frankly extremely time consuming to write. The benefit is that every file can be uniquely identified so if you want to write an installer or updated that chooses this one specific file and only that version of it, and edits it in a specific way, you can reliably do that knowing that you'll always get the right one. However, 