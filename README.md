# ZUI Example Mod for VRising

A simple example mod demonstrating how to use the **ZUI API** for VRising mods.

**?? Purpose:** This mod showcases how to register categories and buttons with ZUI's API.

## ?? Features

This example mod demonstrates:
- ? **ZUI Soft Dependency** - Works with or without ZUI installed
- ? **Category Registration** - Using `ZUI.AddCategory()`
- ? **Button Registration** - Using `ZUI.AddButton()`
- ? **Multiple Categories** - Shows how to organize buttons into groups

## ?? Requirements

- **VRising** game installed
- **BepInEx 6.0.0-be.733** (IL2CPP version)
- **ZUI Core Mod** (REQUIRED for this showcase)
- **.NET 6.0** SDK for building

## ??? Setup Instructions

### 1. Build the Project

```bash
dotnet build
```

### 2. Deploy

Copy the compiled `ZUIExampleMod.dll` to your VRising BepInEx plugins folder:
```
<VRising Install>/BepInEx/plugins/
```

## ?? What This Does

When you load the game with ZUI installed, this mod:

1. **Sets the plugin name**: `ZUI.SetPlugin("ZUIExample")`
2. **Creates two categories**: "Example" and "Example2"
3. **Adds buttons** to each category

### In the ZUI Interface

You'll see in the Mods menu:
```
?? ZUIExample
  ?? Example
    ?? Test
  ?? Example 2
    ?? Example
```

## ?? The Core Code

Here's the entire implementation in `Plugin.cs`:

```csharp
private void RegisterWithZUI()
{
    // Set your plugin name
    ZUI.SetPlugin("ZUIExample");

    // Example category
    ZUI.AddCategory("Example");
    ZUI.AddButton("Test", ".ignorethis");

    // Example 2 category
    ZUI.AddCategory("Example2");
    ZUI.AddButton("Example", ".ignorethis");
   
    LogInstance.LogInfo("Registered with ZUI successfully!");
}
```

**That's it!** Simple and clean.

## ?? Project Structure

```
ZUIExampleMod/
??? Plugin.cs              # Main plugin - registers categories & buttons
??? MyPluginInfo.cs        # Plugin metadata
??? ZUIExampleMod.csproj   # Project configuration
```

## ?? How to Adapt This for Your Mod

### Step 1: Set Your Plugin Name

```csharp
ZUI.SetPlugin("YourModName");
```

This creates a folder in the Mods menu with your mod's name.

### Step 2: Add Categories

```csharp
ZUI.AddCategory("Settings");
ZUI.AddCategory("Features");
ZUI.AddCategory("Admin Tools");
```

Categories organize your buttons into groups.

### Step 3: Add Buttons

```csharp
ZUI.AddButton("Enable Feature", ".yourcommand");
ZUI.AddButton("Show Stats", ".stats");
ZUI.AddButton("Reset Data", ".reset");
```

Each button executes a chat command when clicked.

### Full Example

```csharp
private void RegisterWithZUI()
{
    // Your mod name
    ZUI.SetPlugin("MyAwesomeMod");

    // Player features
    ZUI.AddCategory("Player");
    ZUI.AddButton("Heal", ".heal");
    ZUI.AddButton("Show Stats", ".stats");

    // Admin features
    ZUI.AddCategory("Admin");
    ZUI.AddButton("Give Items", ".give");
    ZUI.AddButton("Teleport", ".tp");
   
    LogInstance.LogInfo("Registered with ZUI successfully!");
}
```

## ?? ZUI API Methods Used

### `ZUI.SetPlugin(string pluginName)`
Sets your plugin's display name in ZUI. Call this FIRST before adding categories.

### `ZUI.AddCategory(string categoryName)`
Creates a new category/folder in your plugin's section. Subsequent `AddButton` calls will add buttons to this category.

### `ZUI.AddButton(string buttonText, string command, string tooltip = "")`
Adds a button to the current category.
- **buttonText**: Text displayed on the button
- **command**: Chat command to execute when clicked (e.g., ".heal")
- **tooltip**: (Optional) Hover text

## ?? Troubleshooting

**Mod loads but no UI appears:**
- Make sure ZUI is installed and loaded
- Check console for "? ZUI detected!" message
- Verify ZUI version is compatible

**"ZUI not found" warning:**
- This mod requires ZUI to function
- Install ZUI core mod from the VRising modding community

**Build errors:**
- Make sure .NET 6.0 SDK is installed
- ZUI.dll reference is optional (conditional in csproj)
- Check that all dependencies are correct

## ?? Design Philosophy

This example mod demonstrates **minimalism**:
- ? No complex logic
- ? No command handlers
- ? Just shows how to use the ZUI API
- ? Perfect template for your own mods

Start here, then add your own functionality!

## ?? Next Steps

1. **Clone this example**
2. **Change the plugin name** in `MyPluginInfo.cs`
3. **Add your categories and buttons** in `RegisterWithZUI()`
4. **Implement your chat commands** in your mod
5. **Build and deploy!**

## ?? License

This is example code for educational purposes. Modify and use as needed!

## ?? Credits

- **ZUI API** by Zanakinz
- Example implementation for the VRising modding community

Happy modding! ???
