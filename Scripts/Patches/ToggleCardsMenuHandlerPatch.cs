using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using ToggleCardsCategories.UI;
using UnboundLib;
using UnboundLib.Utils;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ToggleCardsCategories.Patches {
    [HarmonyPatch(typeof(ToggleCardsMenuHandler))]
    internal class ToggleCardsMenuHandlerPatch {
        [HarmonyPatch("ChangeCardColumnAmountMenus")]
        [HarmonyPrefix]
        public static void ChangeCardColumnAmountMenusPrefex(int amount) {
            foreach(var catgory in ToggleCardsCategoriesManager.instance.RegisteredCategories) {
                CardManager.categories.Remove(catgory); // Remove the category so it not try access a non existing "GridLayoutGroup" and error out
            }
        }

        [HarmonyPatch("ChangeCardColumnAmountMenus")]
        [HarmonyPostfix]
        public static void ChangeCardColumnAmountMenusPostfix(int amount) {
            foreach(var catgory in ToggleCardsCategoriesManager.instance.RegisteredCategories) {
                CardManager.categories.Add(catgory); // Then we add back the category so nothing break
            }

            foreach(var category in ToggleCardsCategoryMenu.Menus.Values) {
                category.SetGridSize(amount);
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix() {
            ToggleCardsCategories.Instance.ExecuteAfterSeconds(0.75f, () => {
                Dictionary<string, List<GameObject>> cardObjectsInCategory = (Dictionary<string, List<GameObject>>)ToggleCardsMenuHandler.instance.GetFieldValue("cardObjectsInCategory");
                foreach(var category in ToggleCardsCategoriesManager.instance.RegisteredCategories) {
                    GameObject content = cardObjectsInCategory[category][0].transform.parent.gameObject;

                    List<GameObject> childGameObjects = content.transform.Cast<Transform>()
                                                .Select(t => t.gameObject)
                                                .ToList();

                    if(content.GetComponent<GridLayoutGroup>()) {
                        Component.DestroyImmediate(content.GetComponent<GridLayoutGroup>());

                        var group = content.AddComponent<VerticalLayoutGroup>();
                        group.padding = new RectOffset(5, 5, 5, 5);

                        content.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                    }

                    foreach(var childGameObject in childGameObjects) {
                        IToggleCardCategory toggleCardCategoryInfo = childGameObject.transform.GetChild(2).GetComponent<IToggleCardCategory>();
                        string categoryName = toggleCardCategoryInfo?.GetCardCategoryInfo().Name ?? "Unknow";

                        ToggleCardsCategoryMenu toggleCardsCategory = ToggleCardsCategoryMenu.AddToggleCardToCategory(content, childGameObject, categoryName, category);
                        if(toggleCardCategoryInfo != null && toggleCardCategoryInfo.GetCardCategoryInfo().Priority != null) {
                            toggleCardsCategory.Priority = toggleCardCategoryInfo.GetCardCategoryInfo().Priority.Value;
                        }
                    }
                }
            });
        }

    }
}
