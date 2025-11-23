During the 1.1.0 update of the Phobos Starhopper package, multiple files (meshes, shaders, textures, materials etc) have changed considerably (both in content and file location).
This was done in order to align with my new asset organization standard. This insures that common assets (files shared between packages, mainly scripts and textures/materials) now have the same location inside the VattalusAssets folder.

Thus, the new package version (VattalusAssets>PhobosStarhopper) uses file references from the VattalusAssets>Common folder, not the VattalusAssets>Extra folder.

This, theoretically, should not have broken your implementation of the assets. However, it has caused some file/folder duplication in your project. Some of auxiliary assets are now split between 2 folders (new ones in Common and old ones in Extra)

Guide to clean up the project:

Step 1: Move all assets from the VattalusAssets>VattalusAssets_PhobosStarhopper folder to the VattalusAssets>PhobosStarhopper
Step 2: Move all assets from the Extra folder to Common folder.
Step 3: The old Phobos materials are now deprecated, make sure you use the new trimsheet materials found in VattalusAssets>Common>Materials>Trimsheet

What has changed exactly?
- All files have moved from the VattalusAssets>VattalusAssets_PhobosStarhopper folder to VattalusAssets>PhobosStarhopper
- All Phobos meshes now use a different set of trimsheet materials (found in VattalusAssets>Common>Materials>Trimsheet)
- Scripts have been updated and moved from the VattalusAssets>Extra folder to the VattalusAssets>Common>Scripts
- The VattalusFirstPersonCamera prefab has been removed and replaced by the new version in VattalusAssets>Common>Prefabs

In short, this update does not add functionality or visuals, everything should work/look the same, though with new/updated assets, which will be shared between other packages in my library.
I have also included the original unitypackage file in the current package in case you need it for anything.


I apologize for any inconveniences caused.
If you need any support with fixing any broken references and such, don't hesitate to contact me at VattalusAssets@gmail.com