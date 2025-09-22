using BepInEx;
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
        public static List<GridLayoutGroup> Groups = new List<GridLayoutGroup>();

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

            SetGridsSize(amount);
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

                    Component.DestroyImmediate(content.GetComponent<GridLayoutGroup>());

                    var group = content.AddComponent<VerticalLayoutGroup>();
                    group.padding = new RectOffset(5, 5, 5, 5);

                    var categoriesGroup = new GameObject("Categories Content", typeof(VerticalLayoutGroup));
                    var categoriesGroupGrid = categoriesGroup.GetComponent<VerticalLayoutGroup>();
                    categoriesGroup.transform.SetParent(content.transform, false);
                    categoriesGroupGrid.spacing = 5;

                    var cardsGroup = new GameObject("Cards Content", typeof(GridLayoutGroup));
                    var cardsGroupGrid = cardsGroup.GetComponent<GridLayoutGroup>();
                    cardsGroup.transform.SetParent(content.transform, false);
                    Groups.Add(cardsGroupGrid);

                    content.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

                    foreach(var childGameObject in childGameObjects) {
                        IToggleCardCategory toggleCardCategoryInfo = childGameObject.transform.GetChild(2).GetComponent<IToggleCardCategory>();
                        if(toggleCardCategoryInfo == null || toggleCardCategoryInfo.GetCardCategoryInfo().Name.IsNullOrWhiteSpace()) {
                            childGameObject.transform.SetParent(cardsGroup.transform);
                        } else {
                            string categoryName = toggleCardCategoryInfo.GetCardCategoryInfo().Name;

                            ToggleCardsCategoryMenu toggleCardsCategory = ToggleCardsCategoryMenu.AddToggleCardToCategory(categoriesGroup, childGameObject, categoryName, category);
                            if(toggleCardCategoryInfo != null && toggleCardCategoryInfo.GetCardCategoryInfo().Priority != null) {
                                toggleCardsCategory.Priority = toggleCardCategoryInfo.GetCardCategoryInfo().Priority.Value;
                            }
                        }
                    }
                }
            });
        }

        private static void SetGridsSize(int amount) {
            Vector2 cellSize = new Vector2(220, 300);
            float localScale = 1.5f;

            if(amount > 3) {
                switch(amount) {
                    case 4: {
                            cellSize = new Vector2(170, 240);
                            localScale = 1.2f;
                            break;
                        }
                    default: {
                            cellSize = new Vector2(136, 192);
                            localScale = 0.9f;
                            break;
                        }
                    case 6: {
                            cellSize = new Vector2(112, 158);
                            localScale = 0.75f;
                            break;
                        }
                    case 7: {
                            cellSize = new Vector2(97, 137);
                            localScale = 0.65f;
                            break;
                        }
                    case 8: {
                            cellSize = new Vector2(85, 120);
                            localScale = 0.55f;
                            break;
                        }
                    case 9: {
                            cellSize = new Vector2(75, 106);
                            localScale = 0.45f;
                            break;
                        }
                    case 10: {
                            cellSize = new Vector2(68, 96);
                            localScale = 0.4f;
                            break;
                        }
                }
            }

            foreach(var grid in Groups) {
                grid.cellSize = cellSize;
                grid.constraintCount = amount;

                List<Transform> cardsInMenu = grid.transform.Cast<Transform>().ToList();

                foreach(var cardContent in cardsInMenu.Select(cardTransform => cardTransform.GetChild(2).gameObject.GetOrAddComponent<RectTransform>())) {
                    cardContent.transform.GetChild(2).localScale = (localScale * Vector3.one * 10);
                }
            }
        }
    }
}
