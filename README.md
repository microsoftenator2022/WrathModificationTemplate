### This fork has the following changes
* Owlcat.Runtime.Visual.dll has been converted to local package com.owlcat.visual
* Added required packages to manifest to automatically download them.
* Play Mode now works
* Scene View now shows shaded UI objects (kuru)
* Gizmos now are displayed (kuru)
* Converted build script to use MSBuild (bubbles)
* Added ProjectFilePostprocessor class to ensure proper csproj settings. Upgrades framework to 4.8.1 and forces portable PDB output.

Getting started
===============

1. Open the project using **Unity 2020.3.38f1**
    * Unity console will show many compiler errors, but **don't panic**!
    * Click **Modification Tools -> Setup project** menu entry and choose _**Pathfinder: Wrath of the Righteous** installation folder_ in the dialog that will appear
    * If Unity shows you **API Update Required** dialog click **No Thanks**
    * Close and reopen project
    * Click **Modification Tools -> Setup render pipeline** menu entry
    * Go to the the Assets\Reources folder and right click and create Position Based Dynamics Config
    * Project is now ready
      
Features
========

All content of your modification must be placed in folder with **Modification** scriptable object or it's subfolders.

### Scripts

All of your scripts must be placed in assemblies (in folder with ***.asmdef** files or it's subfolders). **Never** put your scripts (except Editor scripts) in other places.

### Content

All of your content (assets, prefabs, scenes, sprites, etc) must be placed in **_your-modification-name_/Content** folder.

### Blueprints

Blueprints are JSON files which represent serialized version of static game data (classes inherited from **SimpleBlueprint**).

* Blueprints must have file extension ***.jbp** and must be situated in **_your-modification-name_/Blueprints** folder.
    * _example: Examples/Basics/Blueprints/TestBuff.jbp_

    ```json5
    // *.jbp file format
    {
        "AssetId": "unity-file-guid-from-meta", // "42ea8fe3618449a5b09561d8207c50ab" for example
        "Data": {
            "$type": "type-id, type-name", // "618a7e0d54149064ab3ffa5d9057362c, BlueprintBuff" for example
            
            // type-specific data
        }
    }
    ```

    * if you specify **AssetId** of an existing blueprint (built-in or from another modification) then the existing blueprint will be replaced

* For access to metadata of all built-in blueprints use this method
    ```C#
    // read data from <WotR-installation-path>/Bundles/cheatdata.json
    // returns object {Entries: [{Name, Guid, TypeFullName}]}
    BlueprintList Kingmaker.Cheats.Utilities.GetAllBlueprints();
    ```

* You can write patches for existing blueprints: to do so, create a ***.patch** JSON file in **_your-modification-name_/Blueprints** folder. Instead of creating a new blueprint, these files will modify existing ones by changing only fields that are specified in the patch and retaining everything else as-is.

    * _Example 1: Examples/Basics/Blueprints/ChargeAbility.patch_
    * 
    * _Example 2: Examples/Basics/Blueprints/InvisibilityBuff.patch_ 

    * Connection between the existing blueprint and the patch must be specified in **BlueprintPatches** scriptable object _(right click in folder -> Create -> Blueprints' Patches)_

        * _example: Examples/Basics/BlueprintPatches.asset_
      
    * **OLD**: Newtonsoft.Json's Populate is used for patching (_#ArrayMergeSettings and _#Entries isn't supported)
  
      * https://www.newtonsoft.com/json/help/html/PopulateObject.htm 
  
    * **NEW** (game version 1.1.1): Newtonsoft.Json's Merge is used for patching
  
      * https://www.newtonsoft.com/json/help/html/MergeJson.htm

    ```json5
    // *.patch file format: change icon in BlueprintBuff and disable first component
    {
      "_#ArrayMergeSettings": "Merge", // "Union"/"Concat"/"Replace"
      "m_Icon": {"guid": "b937cb64288636b4c8fd4ba7bea337ea", "fileid": 21300000},
      "Components": [
        {
          "m_Flags": 1
        }
      ]
    }
    ```
  _OR_

    ```json5
    {
      "_#Entries": [
        {
          "_#ArrayMergeSettings": "Merge", // "Union"/"Concat"/"Replace"
          "m_Icon": {"guid": "b937cb64288636b4c8fd4ba7bea337ea", "fileid": 21300000},
          "Components": [
            {
              "m_Flags": 1
            }
          ]
        }
      ]
    }
    ```

### Localization

You can add localized strings to the game or replace existing strings. Create **enGB|ruRU|deDE|frFR|zhCN|esES.json** file(s) in **_your-modification-name_/Localization** folder.

* _example: Examples/Basics/Localizations/enGB.json_

* You shouldn't copy enGB locale with different names if creating only enGB strings: enGB locale will be used if modification doesn't contains required locale.

* The files should be in UTF-8 format (no fancy regional encodings, please!)

```json5
// localization file fromat
{
    "strings": [
        {
            "Key": "guid", // "15edb451-dc5b-4def-807c-a451743eb3a6" for example
            "Value": "whatever-you-want"
        }
    ]
}
```

### Assembly entry point

You can mark static method with **OwlcatModificationEnterPoint** attribute and the game will invoke this method with corresponding _OwlcatModification_ parameter once on game start. Only one entry point per assembly is allowed.

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

```C#
[OwlcatModificationEnterPoint]
public static void EnterPoint(OwlcatModification modification)
{
    ...
}
```

### GUI

Use **OwlcatModification.OnGUI** for inserting GUI to the game. It will be accessible from modifications' window (_ctrl+M_ to open). GUI should be implemented with **IMGUI** (root layout is **vertical**).

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

### Harmony Patching

Harmony lib is included in the game and you can use it for patching code at runtime.

* _example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method) and Examples/Basics/Scripts/Tests/HarmonyPatch.cs_

* [Harmony Documentation](https://harmony.pardeike.net/articles/intro.html)

```C#
OwlcatModification modification = ...;
modification.OnGUI = () => GUILayout.Label("Hello world!");
```

### Storing data

* You can save/load global modification's data or settings with methods _OwlcatModification_.**LoadData** and  _OwlcatModification_.**SaveData**. Unity Serializer will be used for saving this data.

    * _Example: Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.TestData method)_

    ```C#
    [Serialzable]
    public class ModificationData
    {
        public int IntValue;
    }
    ...
    OwlcatModification modification = ...;
    var data = modification.LoadData<ModificationData>();
    data.IntValue = 42;
    modification.SaveData(data);
    ```

* You can save/load per-save modification's data or settings by adding **EntityPartKeyValueStorage** to **Game.Instance.Player**.

    * _Example: Examples/Basics/Scripts/Tests/PerSaveDataTest.cs_

    ```C#
    var data = Game.Instance.Player.Ensure<EntityPartKeyValueStorage>().GetStorage("storage-name");
    data["IntValue"] = 42.ToString();
    ```

### EventBus

You can subscribe to game events with **EventBus.Subscribe** or raise your own event using **EventBus.RaiseEvent**.

* _Example (subscribe): Examples/Basics/Scripts/ModificationRoot.cs (ModificationRoot.Initialize method)_

* Raise your own event:

    ```C#
    interface IModificationEvent : IGlobalSubscriber
    {
        void HandleModificationEvent(int intValue);
    }
    ...
    EventBus.RaiseEvent<IModificationEvent>(h => h.HandleModificationEvent(42))
    ```

### Rulebook Events

* **IBeforeRulebookEventTriggerHandler** and **IAfterRulebookEventTriggerHandler** exists specifically for modifications. These events are raised before _OnEventAboutToTrigger_ and _OnEventDidTigger_ correspondingly.
* Use _RulebookEvent_.**SetCustomData** and _RulebookEvent_.**TryGetCustomData** to store and read your custom RulebookEvent data.

### Resources

_OwlcatModification_.**LoadResourceCallbacks** is invoked every time when a resource (asset, prefab or blueprint) is loaded.

### Game Modes and Controllers

A **Controller** is a class that implements a particular set of game mechanics. It must implementi _IController_ interface.

**Game Modes** (objects of class _GameMode_) are logical groupings of **Controllers** which all must be active at the same time. Only one **Game Mode** can be active at any moment. Each frame the game calls **Tick** method for every **Controller** in active **Game Mode**. You can add your own logic to Pathfinder's main loop or extend/replace existing logic using **OwlcatModificationGameModeHelper**.

* _Example (subscribe): Examples/Basics/Scripts/Tests/ControllersTest.cs_

### Using Pathfinder shaders

Default Unity shaders doesn't work in Pathfinder. Use shaders from **Owlcat** namespace in your materials. If you don't know what you need it's probably **Owlcat/Lit** shader.

### Scenes

You can create scenes for modifications but there is a couple limitations:

* if you want to use Owlcat's MonoBehaviours (i.e. UnitSpawner) you must inherit from it and use child class defined in your assembly

* place an object with component **OwlcatModificationMaterialsInSceneFixer** in every scene which contains Renderers

### Helpers

* Copy guid and file id as json string: _right-click-on-asset -> Modification Tools -> Copy guid and file id_

* Copy blueprint's guid: _right-click-on-blueprint -> Modification Tools -> Copy blueprint's guid_
    
* Create blueprint: _right-click-in-folder -> Modification Tools -> Create Blueprint_

* Find blueprint's type: _Modification Tools -> Blueprints' Types_

### Interactions and dependencies between modifications

Work in progress. Please note that users will be able to change order of mods in the manager. We're planning to provide the ability to specify a list of dependencies for your modification, but it will only work as a hint: the user will be responsible for arranging a correct order of mods in the end.
 
### Testing

* Command line argument **-start_from=_area-name/area-preset-name_** allows you to start game from the specified area without loading main menu.
* Cheat **reload_modifications_data** allows you to reload content, blueprints and localizations. All instantiated objects (prefab instances, for example) stays unchanged.
