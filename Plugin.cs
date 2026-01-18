using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using System;
using System.Linq;
using System.Reflection;

namespace ZUIExampleMod
{
    [BepInPlugin("com.yourname.zuiexample", "ZUI Example Mod", "1.0.0")]
    [BepInDependency("Zanakinz.ZUI", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource LogInstance { get; private set; }
        private static Type _zui;

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
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ZUI");
            _zui = assembly?.GetType("ZUI.API.ZUI");
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
        }

        // --- PART 2: Custom Windows ---
        private void CreateCustomUI()
        {
            // Custom Canvas UI - 500x350
            Call("SetPlugin", "YourPluginName");
            Call("SetTargetWindow", "YourWindowName");
            Call("SetUI", 500, 350);
            Call("HideTitleBar"); // Optional

            Call("SetTitle", "<color=#B30000>ZUIExampleMod</color>");
            Call("AddText", "<color=#61c200>You can create your own custom UI</color>", 15f, 210f);
            Call("AddText", "<color=#61c200>with relative ease using this!</color>", 15f, 230f);
            Call("AddButton", "Test", ".ignorethis", 320f, 220f);
            Call("AddCategory", "<color=#ffd700>README:</color>", 15f, 190f);
            Call("AddCategory", "<color=#ffd700>COMMANDS:</color>", 315f, 190f);
            Call("AddImage", "CHANGENAME.png", 20f, 40f, 460f, 150f);
            Call("AddButton", "Long", ".ignorethis", 20f, 280f, 460f, 20f);
        }
    }
}