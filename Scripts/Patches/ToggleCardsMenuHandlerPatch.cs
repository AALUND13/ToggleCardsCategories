using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ToggleCardsCategories.Extensions;
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
        public static List<ToggleCardsCategoryMenu> MenuToCollapses = new List<ToggleCardsCategoryMenu>();
        public static List<ToggleCardsCategoryMenu> ActiveMenus = new List<ToggleCardsCategoryMenu>();
        public static Queue<Action> DelayActionQueue = new Queue<Action>();
        public static bool AlreadyTrigger = false;
        public static GameObject expandAllButton;

        [HarmonyPatch("ChangeCardColumnAmountMenus")]
        [HarmonyPrefix]
        public static void ChangeCardColumnAmountMenusPrefex(int amount) {
            foreach(var catgory in ToggleCardsCategoriesManager.instance.RegisteredCategories) {
                CardManager.categories.Remove(catgory); // Remove the category so it not try access a non existing "GridLayoutGroup" and error out
            }
        }

        [HarmonyPatch("UpdateVisualsCardObj")]
        [HarmonyPrefix]
        public static bool UpdateVisualsCardObjPrefix(GameObject cardObject) {
            if(ToggleCardsCategoriesManager.instance.Menus.Select(c => c.Value.dropdownButton.gameObject).Contains(cardObject)) {
                return false;
            } else {
                return true;
            }
        }

        [HarmonyPatch("EnableCardsInCategory")]
        [HarmonyPrefix]
        public static void EnableCardsInCategoryPrefex(string category) {
            if(ToggleCardsCategoriesManager.instance.RegisteredCategories.Contains(category)) expandAllButton.SetActive(true);
            else expandAllButton.SetActive(false);
        }

        [HarmonyPatch("ActiveOnSearch")]
        [HarmonyPostfix]
        public static void ActiveOnSearchPostfix(string cardName, ref bool __result) {
            string currentCategory = (string)ToggleCardsMenuHandler.instance.GetFieldValue("currentCategory");
            string currentSearch = (string)ToggleCardsMenuHandler.instance.GetFieldValue("currentSearch");

            if(ToggleCardsCategoriesManager.instance.RegisteredCategories.Contains(currentCategory) &&
                ToggleCardsCategoriesManager.instance.CardsNameToMenuMapping[currentCategory].TryGetValue(cardName, out var menu) &&
                __result
            ) {
                if(currentSearch != "") {
                    DelayActionQueue.Enqueue(() => {
                        if(!menu.viewport.activeSelf) {
                            menu.ExpandCategory();
                            if(!MenuToCollapses.Contains(menu)) {
                                MenuToCollapses.Add(menu);
                            }
                        }

                        var parentMenu = menu.parentCategory;
                        while(parentMenu != null) {
                            if(!parentMenu.viewport.activeSelf) {
                                parentMenu.ExpandCategory();
                                if(!MenuToCollapses.Contains(parentMenu)) {
                                    MenuToCollapses.Add(parentMenu);
                                }
                            }

                            parentMenu = parentMenu.parentCategory;
                        }
                    });

                    if(!ActiveMenus.Contains(menu)) {
                        ActiveMenus.Add(menu);
                    }

                    var activeParentMenu = menu.parentCategory;
                    while(activeParentMenu != null) {
                        if(!ActiveMenus.Contains(activeParentMenu)) {
                            ActiveMenus.Add(activeParentMenu);
                        }

                        activeParentMenu = activeParentMenu.parentCategory;
                    }
                } else {
                    foreach(var menuToCollapse in MenuToCollapses) {
                        if(ActiveMenus.Contains(menuToCollapse)) {
                            ActiveMenus.Remove(menuToCollapse);
                        }
                    }
                }
            }

            if(ToggleCardsCategoriesManager.instance.Menus.Select(c => c.Value.dropdownButton.name).Contains(cardName)) {
                __result = true;
            }
        }

        [HarmonyPatch("ChangeCardColumnAmountMenus")]
        [HarmonyPostfix]
        public static void ChangeCardColumnAmountMenusPostfix(int amount) {
            foreach(var catgory in ToggleCardsCategoriesManager.instance.RegisteredCategories) {
                CardManager.categories.Add(catgory); // Then we add back the category so nothing break
            }

            foreach(var category in ToggleCardsCategoriesManager.instance.Menus.Values) {
                category.SetGridSize(amount);
            }

            SetGridsSize(amount);
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix() {
            var searchBar = ToggleCardsMenuHandler.cardMenuCanvas.transform.Find("CardMenu/Top/InputField").gameObject;
            var toggleAllButton = ToggleCardsMenuHandler.cardMenuCanvas.transform.Find("CardMenu/Top/ToggleAll").gameObject;

            searchBar.GetComponent<TMP_InputField>().onValueChanged.AddListenerLast(value => {
                UnityEngine.Debug.Log("Value Chnage");
                DelayActionQueue.Clear();
                ToggleCardsCategoriesManager.instance.StopAllCoroutines();
                ToggleCardsCategoriesManager.instance.ExecuteAfterFrames(1, () => {
                    UnityEngine.Debug.Log("Delay trigger");
                    while(DelayActionQueue.Count > 0) {
                        UnityEngine.Debug.Log("Invoke Delay Action");
                        var action = DelayActionQueue.Dequeue();
                        action();
                    }

                    foreach(var menuToCollapse in MenuToCollapses.ToList()) {

                        if(!ActiveMenus.Contains(menuToCollapse)) {
                            menuToCollapse.CollapseCategory();
                            MenuToCollapses.Remove(menuToCollapse);
                        }
                        ActiveMenus.Remove(menuToCollapse);
                    }
                });
            });

            expandAllButton = GameObject.Instantiate(toggleAllButton, toggleAllButton.transform.parent);
            expandAllButton.GetComponentInChildren<TextMeshProUGUI>().text = "Expand All";
            expandAllButton.GetComponent<Button>().onClick.RemoveAllListeners();
            expandAllButton.GetComponent<Button>().onClick.AddListener(() => {
                string currentCategory = (string)ToggleCardsMenuHandler.instance.GetFieldValue("currentCategory");
                if(ToggleCardsCategoriesManager.instance.RegisteredCategories.Contains(currentCategory)) {
                    var menus = ToggleCardsCategoriesManager.instance.GetCategoriesByMod(currentCategory);
                    foreach(var menu in menus) {
                        menu.ExpandCategory();
                    }
                }
            });
            expandAllButton.transform.localPosition = new Vector3(416.6898f, 105.7931f, 0);
            expandAllButton.SetActive(false);

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

                            ToggleCardsCategoryMenu toggleCardsCategory = ToggleCardsCategoriesManager.instance.AddToggleCardToCategory(categoriesGroup, childGameObject, categoryName, category);
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
                    cardContent.localScale = (localScale * Vector3.one * 10);
                }
            }
        }
    }
}
