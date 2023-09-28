# ComponentSearchWizard

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that provides a wizard for searching for components that exist on slots in the currently focused world. You can search by type and/or by name, and get a list of references as output.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place (DLL Not Yet Provided) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Resonite logs.

## Usage
The wizard can be found in the DevTip 'Create New' menu under Editors/Component Search Wizard (Mod). <br>

You can drop a component reference into the Component Type field to search for components with that exact type, or check Ignore Type Arguments to search for components with the same base type only. For example, if you drop a ValueField\<bool\>, then with Ignore Type Arguments checked it will search for all ValueField components. <br>
  
The Search Nice Name option will take the component name as, for example, ValueField\<bool\> as opposed to ValueField\`1. <br>
  
You can check Exact Match to match the whole string as you typed it, instead of checking if the component name only contains that string. <br>
  
You can check Spawn Detail Text which gives a text with the component names, their Enabled state, and the hierarchy path to get to them. <br>

## History
Forked from NeosComponentSearchWizard by Nytra, which was forked from NeosLogixCleanupWizard by XDelta.
