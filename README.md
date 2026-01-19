# ZUI Example Mod for VRising

A comprehensive example mod demonstrating how to use the **ZUI API** for VRising mods, including legacy button integration, custom UI window creation, and **server-side packet-based UI creation**.

**Purpose:** This mod showcases three different approaches to using ZUI - adding buttons to the main menu, creating fully custom UI windows, and simulating server-side UI packets.

## Features

This example mod demonstrates:
- **Legacy Button Registration** - Adding buttons to ZUI's main menu
- **Custom UI Windows** - Creating your own standalone UI windows with custom layouts
- **Server-Side UI Packets** - Simulating how server mods can create UIs via chat packets
- **Dynamic Image Loading** - Loading images from web URLs
- **UI Customization** - Positioning elements, adding images, colored text, and more
- **Reflection-based API calls** - Safe integration with ZUI as a soft dependency

## Requirements

- **VRising** game installed
- **BepInEx 6.0.0-be.733** (IL2CPP version)
- **ZUI Core Mod** (REQUIRED for this showcase)
- **.NET 6.0** SDK for building

## Setup Instructions

### 1. Build the Project

```bash
dotnet build
```

### 2. Deploy

Copy the compiled `ZUIExampleMod.dll` to your VRising BepInEx plugins folder:
```
<VRising Install>/BepInEx/plugins/
```

### 3. Add Your Custom Image (Optional)

If you want to test the image functionality:
1. Create a PNG image file
2. Rename it to match what's in the code (or update the code to match your filename)
3. Place it in the appropriate ZUI images directory

## 💡 What This Does

When you load the game with ZUI installed, this mod creates three different UI demonstrations:

### Part 1: 🔘 Legacy Buttons (Main Menu Integration)

Adds a "Simple Stuff" category to ZUI's main menu with buttons:
- **Hello World** - Executes `.say Hello` command
- **Kill Self** - Executes `.kill` command
- **Test Server Packet** - Triggers the server-side UI packet simulation

### Part 2: 🎨 Custom UI Window

Creates a completely custom 500x350 pixel window named "YourWindowName" featuring:
- **Custom title** with colored text
- **Multiple text elements** positioned manually
- **Categories** for organization (README and COMMANDS sections)
- **Buttons** at specific positions
- **Image display** capability

### Part 3: 📡 Server-Side Packet Simulation

Demonstrates how a **server-only mod** would create a UI by sending JSON packets via chat:
- **Dynamic window creation** via packets
- **Image downloading from URLs** (RegisterImage packet)
- **Custom positioned buttons** with size control
- **Automatic window opening** via Open packet

This simulation shows how server mods can create UIs without players needing the mod installed - only ZUI is required on the client.

## 🔧 The Core Code

The plugin uses reflection to safely call ZUI methods, making it work as a soft dependency:

```csharp
private void Call(string name, params object[] args)
{
    if (_zui == null) return;
    var method = _zui.GetMethods(BindingFlags.Public | BindingFlags.Static)
                     .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == args.Length);
    if (method != null) method.Invoke(null, args);
    else LogInstance.LogError($"Could not find ZUI method '{name}' with {args.Length} parameters.");
}
```

### Creating Custom Windows

```csharp
private void CreateCustomUI()
{
    Call("SetPlugin", "YourPluginName");
    Call("SetTargetWindow", "YourWindowName");
    Call("SetUI", 500, 350);  // Width x Height

    Call("SetTitle", "<color=#B30000>ZUIExampleMod</color>");
    Call("AddText", "<color=#61c200>Custom text here</color>", 15f, 210f);
    Call("AddButton", "Test", ".ignorethis", 320f, 220f);
    Call("AddCategory", "<color=#ffd700>README:</color>", 15f, 190f);
    Call("AddImage", "CHANGENAME.png", 20f, 40f, 460f, 150f);
}
```

### Server-Side Packet Approach

```csharp
private void CreateUI_ViaPackets()
{
    // 1. Setup window
    SendPacket("SetPlugin", new Dictionary<string, string> { { "Plugin", "ServerPacketWindow" } });
    SendPacket("SetTargetWindow", new Dictionary<string, string> { { "Window", "RemoteUI" } });
    SendPacket("SetUICustom", new Dictionary<string, string> { { "W", "500" }, { "H", "400" } });
    SendPacket("SetTitle", new Dictionary<string, string> { { "Text", "<color=#00FFFF>Server Streamed UI</color>" } });

    // 2. Register image from URL
    SendPacket("RegisterImage", new Dictionary<string, string> {
        { "Name", "remote_banner.png" },
        { "Url", "https://yourdomain.com/banner.png" }
    });

    // 3. Use the downloaded image
    SendPacket("AddImage", new Dictionary<string, string> {
        { "Img", "remote_banner.png" },
        { "X", "200" }, { "Y", "50" },
        { "W", "100" }, { "H", "100" }
    });

    // 4. Add content
    SendPacket("AddText", new Dictionary<string, string> {
        { "Text", "This image was downloaded from the web!" },
        { "X", "120" }, { "Y", "160" }
    });

    // 5. Custom button with size
    SendPacket("AddButton", new Dictionary<string, string> {
        { "Text", "Wide Server Button" },
        { "Cmd", ".say clicked" },
        { "X", "50" }, { "Y", "200" },
        { "W", "400" }, { "H", "40" }
    });

    // 6. Force open
    SendPacket("Open", new Dictionary<string, string>());
}

private void SendPacket(string type, Dictionary<string, string> data)
{
    var packet = new { Type = type, Plugin = "ServerPacketWindow", Window = "RemoteUI", Data = data };
    string json = JsonSerializer.Serialize(packet);
    string message = "[[ZUI]]" + json;
    
    // In a real server mod: ServerChatUtils.SendSystemMessageToUser(userEntity, message);
    // This example simulates it by directly calling PacketService
}
```

## 📁 Project Structure

```
ZUIExampleMod/
├── Plugin.cs              # Main plugin - creates UI elements
├── MyPluginInfo.cs        # Plugin metadata
└── ZUIExampleMod.csproj   # Project configuration
```

## 🎯 How to Adapt This for Your Mod

### Approach 1: Simple Menu Buttons

Perfect for basic functionality that just needs a few buttons:

```csharp
Call("SetPlugin", "MyMod");
Call("SetTargetWindow", "Main");
Call("AddCategory", "My Features");
Call("AddButton", "Do Something", ".mycommand");
```

### Approach 2: Custom UI Windows

For complex UIs with custom layouts:

```csharp
Call("SetPlugin", "MyMod");
Call("SetTargetWindow", "MyCustomWindow");
Call("SetUI", 600, 400);  // Set window size

// Position elements exactly where you want
Call("AddText", "Welcome!", 10f, 10f);
Call("AddButton", "Click Me", ".command", 10f, 50f, 200f, 30f);
Call("AddImage", "logo.png", 10f, 100f, 580f, 200f);
```

### Approach 3: Server-Side UI (For Server Mods)

Create UIs from the server without requiring client mods:

```csharp
// In your server mod (no ZUI.dll dependency needed)
private void SendZUIPacket(string type, Dictionary<string, string> data)
{
    var packet = new { Type = type, Plugin = "MyServerMod", Window = "ServerUI", Data = data };
    string json = JsonSerializer.Serialize(packet);
    string message = "[[ZUI]]" + json;
    ServerChatUtils.SendSystemMessageToUser(userEntity, message);
}

// Create UI
SendZUIPacket("SetUICustom", new Dictionary<string, string> { { "W", "500" }, { "H", "300" } });
SendZUIPacket("AddButton", new Dictionary<string, string> {
    { "Text", "Server Button" }, { "Cmd", ".command" },
    { "X", "20" }, { "Y", "100" }, { "W", "200" }, { "H", "40" }
});
SendZUIPacket("Open", new Dictionary<string, string>());
```

## 📚 ZUI API Methods Demonstrated

| Method | Description | Usage |
|--------|-------------|-------|
| `SetPlugin(string)` | Sets the plugin identifier | `Call("SetPlugin", "MyMod");` |
| `SetTargetWindow(string)` | Targets "Main" menu or a custom window name | `Call("SetTargetWindow", "Main");` |
| `SetUI(int, int)` | Creates custom window with width × height dimensions | `Call("SetUI", 500, 350);` |
| `SetTitle(string)` | Sets window title (supports HTML color tags) | `Call("SetTitle", "<color=#FF0000>My Window</color>");` |
| `AddCategory(string)` | Adds category label in main menu | `Call("AddCategory", "Admin");` |
| `AddCategory(string, float, float)` | Adds category label at specific X, Y position | `Call("AddCategory", "README:", 15f, 190f);` |
| `AddButton(string, string)` | Adds button to main menu with text and command | `Call("AddButton", "Heal", ".heal");` |
| `AddButton(string, string, float, float)` | Adds button at X, Y position | `Call("AddButton", "Test", ".cmd", 320f, 220f);` |
| `AddButton(string, string, float, float, float, float)` | Adds button with position (X, Y) and size (W, H) | `Call("AddButton", "Long", ".cmd", 20f, 280f, 460f, 20f);` |
| `AddText(string, float, float)` | Adds text at specific X, Y coordinates | `Call("AddText", "Hello!", 15f, 210f);` |
| `AddImage(string, float, float, float, float)` | Adds image with filename, X, Y, width, height | `Call("AddImage", "logo.png", 20f, 40f, 460f, 150f);` |
| `RegisterImage(string, string)` | Registers a web image for download (server-side) | `SendPacket("RegisterImage", ...)` |
| `SetUICustom(int, int)` | Alternative to SetUI (used in packets) | `SendPacket("SetUICustom", ...)` |
| `Open()` | Forces a window to open (server-side) | `SendPacket("Open", ...)` |

### 🎨 Custom UI Design Tool

**Want to design your custom UI visually?**

Use the official ZUI Canvas Designer at **[https://zanakinz.github.io/ZUI](https://zanakinz.github.io/ZUI)**

This interactive tool allows you to:
- Visually position UI elements (buttons, text, images, categories)
- Preview your layout in real-time
- Export code directly for your mod
- Experiment with different window sizes and arrangements

Instead of manually calculating X/Y coordinates, use the designer to drag and drop elements, then copy the generated code into your mod!

## 📐 Positioning System
Coordinates are in pixels from top-left (0, 0):
- **X increases** going right
- **Y increases** going down
- Origin is top-left corner of the window

## 🎨 Color Support

ZUI supports Unity Rich Text color tags:
```csharp
"<color=#FF0000>Red Text</color>"
"<color=#00FF00>Green Text</color>"
"<color=#B30000>Dark Red</color>"
"<color=#ffd700>Gold</color>"
```

## 🔍 Troubleshooting

**Mod loads but no UI appears:**
- Make sure ZUI is installed and loaded first
- Check BepInEx console for errors
- Verify ZUI version compatibility

**"ZUI not found" or mod works without errors but no UI:**
- ZUI is marked as a soft dependency
- The mod will load without ZUI but won't create any UI
- Install ZUI core mod

**Custom window doesn't appear:**
- Check that `SetUI(width, height)` is called before adding elements
- Verify window dimensions are reasonable (100-2000 pixels)
- Make sure `SetTargetWindow()` uses a unique name

**Images not showing:**
- Verify image file exists in ZUI's image directory
- Check filename matches exactly (case-sensitive)
- Supported format: PNG

**Build errors:**
- Ensure .NET 6.0 SDK is installed
- ZUI reference is handled via reflection (no compile-time dependency needed)

## 🎯 Design Philosophy

This example demonstrates:
- **Soft dependency pattern** - Works even if ZUI isn't installed
- **Reflection-based API access** - No compile-time dependency on ZUI.dll
- **Three UI paradigms** - Simple menu integration, complex custom windows, and server-side packets
- **Practical examples** - Real positioning values you can copy and modify
- **Server-side simulation** - Shows how server mods can control client UIs

## 🚶 Next Steps

1. **Clone this example**
2. **Update plugin metadata** in `MyPluginInfo.cs`
3. **Choose your approach:**
   - Simple menu buttons → Modify `CreateSimpleUI()`
   - Custom window → Modify `CreateCustomUI()`
   - Server-side UI → Adapt `CreateUI_ViaPackets()` for your server mod
4. **Adjust positions and sizes** to fit your needs
5. **Add your own images** (local or web URLs)
6. **Implement command handlers** in your mod
7. **Build and deploy!**

## 🙏 Credits

- **ZUI API** by Zanakinz
- Example implementation for the VRising modding community

## 🤖 AI Disclosure

- AI was used in creation of readme's
- AI was used for debugging

## 📜 License

This is example code for educational purposes. Modify and use as needed!

- **ZUI API** by Zanakinz
- Example implementation for the VRising modding community

Happy modding!