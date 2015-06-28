# Asset Prefab(+Building) AI Changer

This mod adds a drop down list to the Asset Property editor that lets you choose any AI for the asset you are editing.

>**Warning** - `WorksAsBuilding()` and `WorksAsNet()` checks are not implemented.
>
>It is not known if assigning wildly inappropriate AIs to assets will break them.

>Broken assets will fail to load and cannot be fixed with the asset editor.

(While I won't stop you) from assigning the `PetAI` to your kennel, or a `ResidentialBuildingAI` to a campervan; or any other invalid combination of (Net, Building, Citizen, Vehicle, etc.) -- It is strongly recommended that illogical changes be made to a new save file. 
Duplicate files can be deleted; backups made after the fact are extraordinarily unlikely to fix anything.

###Purpose
The main intended use of this mod is to ease the creation of assets with custom AI classes.

###Usage
You do *not* need to know the exact name of the AI class. The drop down list is populated from the assemblies loaded when the asset editor is opened, then ordered by base-classes.

Load an asset with an AI to be replaced. If the asset lacks a suitable PrefabAI the PrefabAISelector panel be hidden.

Click on the name of the desired AI type from the drop down list or mousewheel until it is selected. The checkbox will deselect itself when the selected type does not match the currently loaded type.  This was done intentionally as accidental mousewheeling could cause values to be lost accidentally.

Checking the checkbox (it will light) will attempt to replace the current prefab with a one of the specified type, and 'refresh' the properties panel.  For prefab types for which the properties are suppressed will hide the panel and will not load properties, but will accept the AI specified.

Existing properties on the current AI instance are carried over to the new AI as far as possible, but this part is still very much a work-in-progress. It's probably a good idea to select the proper AI as early as possible to avoid problems.

## Using custom AI classes

If you intend to use a custom prefab AI, make sure to remember the namespace of your mod so that you may spot it in the list. For example, use MySuperMod.SuperAI. 

This is not neccessary for the stock AIs as they all seem to be in the root namespace.

## Source code & Issues

**WARNING:** This is just out of the first development phase, don't expect everything to work flawlessly. Some things might break.

The source code of this mod is available at https://github.com/dluechoy/BuildingAIChanger/tree/DropDown

The source code the mod on which this built is availble from https://github.com/cerebellum42/BuildingAIChanger

If you want to report an issue, please do use the Github issues as I may not always read the steam workshop comments.
