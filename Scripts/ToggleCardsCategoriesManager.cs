using System;
using System.Collections.Generic;
using System.Linq;
using ToggleCardsCategories.UI;
using UnboundLib.Utils;
using UnityEngine;

namespace ToggleCardsCategories {
    public class ToggleCardsCategoriesManager : MonoBehaviour {
        public static ToggleCardsCategoriesManager instance { get; internal set; }
        
        public readonly Dictionary<string, ToggleCardsCategoryMenu> Menus = new Dictionary<string, ToggleCardsCategoryMenu>();
        internal readonly Dictionary<string, Dictionary<string, ToggleCardsCategoryMenu>> CardsNameToMenuMapping 
            = new Dictionary<string, Dictionary<string, ToggleCardsCategoryMenu>>();

        public IReadOnlyList<string> RegisteredCategories => registeredCategories.AsReadOnly();
        private List<string> registeredCategories = new List<string>();

        public void RegisterCategories(string category) {
            if(registeredCategories.Contains(category)) return;
            registeredCategories.Add(category);
        }

        public ToggleCardsCategoryMenu[] GetCategoriesByMod(string ModPrefix) {
            string modPrefix = ModPrefix.Split('/').First();
            return Menus.Where(menu => menu.Key.StartsWith(modPrefix)).Select(m => m.Value).ToArray();
        }

        public ToggleCardsCategoryMenu AddToggleCardToCategory(GameObject parent, GameObject toggleCard, string categoryPath, string modPrefix) {
            if(!Menus.ContainsKey($"{modPrefix}/{categoryPath}")) CreateToggleCardsCategoryMenu(parent, categoryPath, modPrefix);
            var targetMenu = Menus[$"{modPrefix}/{categoryPath}"];

            targetMenu.AddToggleCardToCategory(toggleCard);
            if(!CardsNameToMenuMapping.ContainsKey(modPrefix)) CardsNameToMenuMapping.Add(modPrefix, new Dictionary<string, ToggleCardsCategoryMenu>());
            CardsNameToMenuMapping[modPrefix].Add(toggleCard.name, targetMenu);

            return targetMenu;
        }

        private void CreateToggleCardsCategoryMenu(GameObject parent, string categoryPath, string modPrefix) {
            string[] pathSegments = categoryPath.Split('/');

            string fullPath = pathSegments[0];
            GameObject currentParent = parent;
            ToggleCardsCategoryMenu previousMenu = null;

            for(int i = 0; i < pathSegments.Length; i++) {
                string segment = pathSegments[i];
                if(i > 0) fullPath += $"/{segment}";

                if(Menus.ContainsKey($"{modPrefix}/{fullPath}")) {
                    var menu = Menus[$"{modPrefix}/{fullPath}"];
                    currentParent = menu.categoriesContent;

                    previousMenu = menu;
                } else if(!Menus.ContainsKey($"{modPrefix}/{fullPath}")) {
                    GameObject menuInstance = GameObject.Instantiate(ToggleCardsCategoryMenu.Prefab);
                    menuInstance.transform.SetParent(currentParent.transform, false);

                    var newMenu = menuInstance.GetComponent<ToggleCardsCategoryMenu>();
                    newMenu.categoryText.text = segment;
                    newMenu.categoryDepth = i;

                    Menus.Add($"{modPrefix}/{fullPath}", newMenu);

                    if(previousMenu) {
                        previousMenu.categories.Add(newMenu);
                        previousMenu.categoriesContent.SetActive(true);
                        newMenu.parentCategory = previousMenu;
                    }

                    newMenu.viewport.SetActive(false);
                    newMenu.dropdownButton.image.sprite = newMenu.dropdownCloseImage;

                    currentParent = newMenu.categoriesContent;
                    previousMenu = newMenu;
                }
            }
        }
    }
}
