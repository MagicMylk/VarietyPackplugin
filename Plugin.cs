using BepInEx;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalLib;
using LethalLib.Modules;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using NetworkPrefabs = LethalLib.Modules.NetworkPrefabs;

namespace VarietyPackPlugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {

        

        public static Plugin instance;

        private void Awake()
        {
           
            // no clue what this does but Its here
            if (instance == null)
            {
                instance = this;
            }
            

            
            // Load the asset bundle
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetBundle MoneyBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "money"));


            //if shit broken = log that i fucked up
            if (MoneyBundle == null)
            {
                Logger.LogInfo("Assets failed to load");
            }


            // Load items from bundle
            Item InfinityItem = MoneyBundle.LoadAsset<Item>("Assets/Bundle Assets/Infinity V2/Infinity/PurpleInfinity.asset");
            Item MoneyItem = MoneyBundle.LoadAsset<Item>("Assets/Bundle Assets/MoneyPrefab/MoneyData.asset");
            Item FingerItem = MoneyBundle.LoadAsset<Item>("Assets/Bundle Assets/Finger Asset/Finger Data.asset");
            Item BillItem = MoneyBundle.LoadAsset<Item>("Assets/Bundle Assets/Single bill prefab/Single Bill Data.asset");
            Item Coin = MoneyBundle.LoadAsset<Item>("Assets/Bundle Assets/Coin Asset/Coin Data.asset");
            


            InfinityItem.itemName = "Infinity";

            TerminalNode Infinitynode = ScriptableObject.CreateInstance<TerminalNode>();
            Infinitynode.clearPreviousText = true;
            Infinitynode.displayText = "Condensed Infinity that fits in the palm of your hand...surprisingly light?";


            


            //Register all that shit bb
            Items.RegisterShopItem(InfinityItem,null,null,Infinitynode,0);
            NetworkPrefabs.RegisterNetworkPrefab(InfinityItem.spawnPrefab);
            Items.RegisterScrap(InfinityItem, 1, Levels.LevelTypes.All);


            NetworkPrefabs.RegisterNetworkPrefab(MoneyItem.spawnPrefab);
            Items.RegisterScrap(MoneyItem, 20, Levels.LevelTypes.All);
            Items.RegisterShopItem(MoneyItem, 0);

            NetworkPrefabs.RegisterNetworkPrefab(FingerItem.spawnPrefab);
            Items.RegisterScrap(FingerItem,2, Levels.LevelTypes.All);
            Items.RegisterShopItem(FingerItem, 0);


            NetworkPrefabs.RegisterNetworkPrefab(BillItem.spawnPrefab);

            NetworkPrefabs.RegisterNetworkPrefab(Coin.spawnPrefab);
            Items.RegisterScrap(Coin,15, Levels.LevelTypes.All);
            Items.RegisterShopItem(Coin, 0);

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            NetcodePatcher();
        }

        //For making netcode do netcode stuff
        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
