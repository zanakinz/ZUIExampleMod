using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using System.Linq;

namespace ZUIExampleMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("Zanakinz.ZUI", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource LogInstance { get; private set; }

        public override void Load()
        {
            LogInstance = Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loading...");

            // Check if ZUI is available and register UI
            if (IsZUIAvailable())
            {
                Log.LogInfo("? ZUI detected! Registering categories and buttons...");
                RegisterWithZUI();
            }
            else
            {
                Log.LogWarning("? ZUI not found. This is a UI showcase mod - ZUI is required.");
            }

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} loaded successfully!");
        }

        private bool IsZUIAvailable()
        {
            try
            {
                return IL2CPPChainloader.Instance.Plugins.ContainsKey("Zanakinz.ZUI");
            }
            catch
            {
                return false;
            }
        }

        private void RegisterWithZUI()
        {
            try
            {
                // Get ZUI assembly and API.ZUI class (not Plugin class!)
                var zuiAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ZUI");

                if (zuiAssembly == null)
                {
                    LogInstance.LogError("Could not find ZUI assembly");
                    return;
                }

                // The API methods are in ZUI.API.ZUI class, not ZUI.Plugin
                var zuiApiType = zuiAssembly.GetType("ZUI.API.ZUI");
                if (zuiApiType == null)
                {
                    LogInstance.LogError("Could not find ZUI.API.ZUI type");
                    return;
                }

                // Get methods from the API class
                var setPluginMethod = zuiApiType.GetMethod("SetPlugin", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var addCategoryMethod = zuiApiType.GetMethod("AddCategory", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var addButtonMethod = zuiApiType.GetMethod("AddButton", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (setPluginMethod == null || addCategoryMethod == null || addButtonMethod == null)
                {
                    LogInstance.LogError("Could not find ZUI API methods");
                    LogInstance.LogError($"SetPlugin: {setPluginMethod != null}, AddCategory: {addCategoryMethod != null}, AddButton: {addButtonMethod != null}");
                    return;
                }

                // Set your plugin name
                setPluginMethod.Invoke(null, new object[] { "ZUIExample" });

                // Example category
                addCategoryMethod.Invoke(null, new object[] { "Example" });
                addButtonMethod.Invoke(null, new object[] { "Test", ".ignorethis", "" });

                // Example 2 category
                addCategoryMethod.Invoke(null, new object[] { "Example2" });
                addButtonMethod.Invoke(null, new object[] { "Example", ".ignorethis", "" });

                LogInstance.LogInfo("Registered with ZUI successfully!");
            }
            catch (System.Exception ex)
            {
                LogInstance.LogError($"Failed to register with ZUI: {ex.Message}");
                LogInstance.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
