using BepInEx;
using BepInEx.Logging;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;

namespace mt2_freecompany.Plugin
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);

        public void Awake()
        {
            Logger = base.Logger;

            var builder = Railhead.GetBuilder();
            builder.Configure(
                MyPluginInfo.PLUGIN_GUID,
                c =>
                {
                    // La lista de rutas JSON que carga el clan.
                    // Anadir aqui CADA fichero nuevo y recompilar: si no esta en
                    // esta lista, el juego no lo lee. Ver docs/64.
                    c.AddMergedJsonFile(
                        "json/plugin.json"
                    );
                }
            );

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
