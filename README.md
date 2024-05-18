# Sapphire-Extract
This is a rewrite of Sapphire Extract previously in java. This project will extract files from various video games. Only some of the formats from java have been reimplemented. Currently implemented formats can be found in the plugins folder.

Formats which will not be covered at this time as they are just renames:
And Then There Were None: acg->jpg
And Then There Were None: acv->wav
3DS Camera: mpo->jpg   Technically a container with 2 views, but good enough to view 2d
Chris Sawyer's Locomotion: dat->wav   (Only some files)
Syberia: syb->bik
Oregon Trail: M->wav

TODO: cleanup
To add a new format, place the template plugin zip under "Documents\Visual Studio 2019\Templates\ProjectTemplates"
Add a new project of the same name and select the location in the plugins subdir.
//Right click on the project in the solution explorer and go to add->project dependencies
//Check contract and common
Right click and rename the cs file
