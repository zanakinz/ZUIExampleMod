using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json; // Required for Packet Serialization

namespace ZUIExampleMod
{
    [BepInPlugin("com.yourname.zuiexample", "ZUI Example Mod", "1.2.0")]
    [BepInDependency("Zanakinz.ZUI", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource LogInstance { get; private set; }
        private static Type _zui;
        private static Type _packetService; // Reflection reference to PacketService

        public override void Load()
        {
            LogInstance = Log;
            Log.LogInfo("Loading Example Mod...");

            if (InitZUI())
            {
                CreateSimpleUI();
                CreateCustomUI();
            }
        }

        private bool InitZUI()
        {
            if (!IL2CPPChainloader.Instance.Plugins.ContainsKey("Zanakinz.ZUI")) return false;

            // Get ZUI Assembly
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ZUI");

            // Get API Type
            _zui = assembly?.GetType("ZUI.API.ZUI");

            // Get PacketService Type (For simulation purposes only)
            _packetService = assembly?.GetType("ZUI.Services.PacketService");

            return _zui != null;
        }

        private void Call(string name, params object[] args)
        {
            if (_zui == null) return;
            var method = _zui.GetMethods(BindingFlags.Public | BindingFlags.Static)
                             .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == args.Length);
            if (method != null) method.Invoke(null, args);
            else LogInstance.LogError($"Could not find ZUI method '{name}' with {args.Length} parameters.");
        }

        // --- PART 1: Legacy Buttons (In Main Menu) ---
        private void CreateSimpleUI()
        {
            Call("SetPlugin", "ZUI Example");
            Call("SetTargetWindow", "Main");

            Call("AddCategory", "Simple Stuff");
            Call("AddButton", "Hello World", ".say Hello");
            Call("AddButton", "Kill Self", ".kill");

            // New Button to test the Packet Logic
            Call("AddButtonWithCallback", "Test Server Packet", (Action)CreateUI_ViaPackets, "Simulate receiving a UI packet from server");
        }

        // --- PART 2: Custom Windows (Client-Side Reflection) ---
        private void CreateCustomUI()
        {
            // Custom Canvas UI - 500x350
            Call("SetPlugin", "YourPluginName");
            Call("SetTargetWindow", "YourWindowName");
            Call("SetUI", 500, 350);
            Call("HideTitleBar");

            Call("SetTitle", "<color=#B30000>ZUIExampleMod</color>");
            Call("AddText", "<color=#61c200>You can create your own custom UI</color>", 15f, 210f);
            Call("AddText", "<color=#61c200>with relative ease using this!</color>", 15f, 230f);
            Call("AddButton", "Test", ".ignorethis", 320f, 220f);
            Call("AddCategory", "<color=#ffd700>README:</color>", 15f, 190f);
            Call("AddCategory", "<color=#ffd700>COMMANDS:</color>", 315f, 190f);

            // Requires "CHANGENAME.png" to be in local plugins/ZUIExampleMod/Sprites/ folder
            Call("AddImage", "CHANGENAME.png", 20f, 40f, 460f, 150f);
            Call("AddButton", "Long", ".ignorethis", 20f, 280f, 460f, 20f);
        }

        // --- PART 3: Packet Simulation (Server-Side Approach) ---
        // This demonstrates how a Server-Only mod sends UI data + Web Images.
        private void CreateUI_ViaPackets()
        {
            // 1. Context & Setup
            SendPacket("SetPlugin", new Dictionary<string, string> { { "Plugin", "ServerPacketWindow" } });
            SendPacket("SetTargetWindow", new Dictionary<string, string> { { "Window", "RemoteUI" } });
            SendPacket("SetUICustom", new Dictionary<string, string> { { "W", "500" }, { "H", "400" } });
            // Note: We are NOT calling HideTitleBar, so we use SetTitle
            SendPacket("SetTitle", new Dictionary<string, string> { { "Text", "<color=#00FFFF>Server Streamed UI</color>" } });

            // This tells the client to download this image and cache it as "remote_banner.png"
            // Replace URL with a real hosted image. Using a placeholder for example.
            SendPacket("RegisterImage", new Dictionary<string, string> {
                { "Name", "remote_banner.png" },
                { "Url", "https://raw.githubusercontent.com/zanakinz/ZUI/refs/heads/master/Images/logo.png" } // Example icon
            });

            // 3. Use the Downloaded Image
            SendPacket("AddImage", new Dictionary<string, string> {
                { "Img", "button.png" }, // Must match Name above
                { "X", "200" }, { "Y", "50" },
                { "W", "100" }, { "H", "100" }
            });

            // 4. Add Text
            SendPacket("AddText", new Dictionary<string, string> {
                { "Text", "This image was downloaded from the web!" },
                { "X", "120" }, { "Y", "160" }
            });

            // 5. Custom Sized Button (Default Sprite)
            SendPacket("AddButton", new Dictionary<string, string> {
                { "Text", "Wide Server Button" }, { "Cmd", ".say clicked" },
                { "Img", "" }, // Empty string = Default ZUI Button Look
                { "X", "50" }, { "Y", "200" },
                { "W", "400" }, { "H", "40" }
            });

            // 6. Force Open
            SendPacket("Open", new Dictionary<string, string>());
        }

        private void SendPacket(string type, Dictionary<string, string> data)
        {
            var packet = new
            {
                Type = type,
                Plugin = "ServerPacketWindow",
                Window = "RemoteUI",
                Data = data
            };

            string json = JsonSerializer.Serialize(packet);
            string message = "[[ZUI]]" + json;

            Log.LogInfo($"[Simulating Packet]: {message}");

            // SIMULATION: Inject directly into PacketService via Reflection
            // In a real Server Mod, you would use: ServerChatUtils.SendSystemMessageToUser(...)
            if (_packetService != null)
            {
                var method = _packetService.GetMethod("TryProcessPacket", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { message });
                }
            }
        }
    }
}