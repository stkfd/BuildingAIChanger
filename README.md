# Prefab AI Changer

Originally Building AI Changer, after merge of Snow_Cat's (D Lue Choy) additions and subsequent rewrite, generic Prefab AI changer.

This mod adds a drop down list to the Asset Property editor that lets you choose any AI for the asset you are editing.

**Warning** - `WorksAsBuilding()` and `WorksAsNet()` checks are not implemented.

Assigning wildly inappropriate AIs will break an asset, cause exceptions and other weird things. Broken assets will fail to load and cannot be fixed with the asset editor.

You can try to assign inappropriate AI types (a VehicleAI to a building, etc) but chances are something will break horribly. So if you do, backup your asset **before** you try.

If you want to create a custom Building AI, create a building and assign your new AI to, if you create a custom VehicleAI, start with a stock VehicleAI and change that, etc.

## Purpose

The main intended use of this mod is to ease the creation of assets with custom AI classes.

### Usage

Just select the name of the new AI class and click "Apply" after you have made your selection. When you click Apply the new AI will actually be applied to the asset and refresh the properties panel with the new set of options for the selected AI. If the previous and  new AI share properties, their values will carry over.

## Using custom AI classes

You can assign custom AI classes with this, mod. They will start with the namespace of your mod in the AI list.

## Source code & Issues

The source code of this mod is available at https://github.com/cerebellum42/BuildingAIChanger

If you want to report an issue, please use the Github issues if you can as I may not always read the steam workshop comments. I will read your comment eventually, but a Github issue will notify me right away.
