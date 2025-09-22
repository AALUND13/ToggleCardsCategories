using BepInEx;
using HarmonyLib;
using ToggleCardsCategories.UI;
using UnityEngine;

namespace ToggleCardsCategories {
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(modId, modName, "1.0.1")]
    [BepInProcess("Rounds.exe")]
    public class ToggleCardsCategories : BaseUnityPlugin {
        private const string modId = "com.aalund13.rounds.toggle_cards_categories";
        private const string modName = "ToggleCardsCategories";

        internal static ToggleCardsCategories Instance;
        internal static AssetBundle Assets;

        void Awake() {
            Instance = this;
            new Harmony(modId).PatchAll();

            Assets = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("togglecardscategories_assets", typeof(ToggleCardsCategories).Assembly);
            ToggleCardsCategoriesManager.instance = gameObject.AddComponent<ToggleCardsCategoriesManager>();
            ToggleCardsCategoryMenu.Prefab = Assets.LoadAsset<GameObject>("CardsCatagory");

            Debug.Log($"{modName} loaded!");
        }
        void Start() {
            Debug.Log($"{modName} started!");
        }
    }
}