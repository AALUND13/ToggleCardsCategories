using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Utils;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ToggleCardsCategories.UI {
    public class ToggleCardsCategoryMenu : MonoBehaviour {
        public static GameObject Prefab;

        public static Dictionary<string, ToggleCardsCategoryMenu> Menus = new Dictionary<string, ToggleCardsCategoryMenu>();

        public int Priority {
            get => gameObject.GetOrAddComponent<LayoutElement>().layoutPriority;
            set => gameObject.GetOrAddComponent<LayoutElement>().layoutPriority = value;
        }

        private bool isDisable {
            get {
                if(categories.Count > 0) {
                    if(categories.All(c => c.isDisable)) return true;
                }
                if(cardsInfo.Count > 0) {
                    var cardNames = cardsInfo.Select(ci => ci.name).ToList();
                    var relevantCards = CardManager.cards.Values
                        .Where(c => c.category == "AAC" && cardNames.Contains(c.cardInfo.name));
                    if(relevantCards.All(c => !c.enabled)) return true;
                }
                return false;
            }
        }

        [SerializeField] private Sprite dropdownOpenImage;
        [SerializeField] private Sprite dropdownCloseImage;
        [SerializeField] private Button dropdownButton;
        [SerializeField] private Toggle toggleCardsButton;

        [SerializeField] private GameObject cardsContent;
        [SerializeField] private GameObject categoriesContent;
        [SerializeField] private GameObject viewport;

        [SerializeField] private GameObject darkenEffect;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;


        private List<CardInfo> cardsInfo = new List<CardInfo>();
        private List<ToggleCardsCategoryMenu> categories = new List<ToggleCardsCategoryMenu>();

        private List<GameObject> categoryContent = new List<GameObject>();
        private List<GameObject> categoryCardsContent = new List<GameObject>();

        private int categoryDepth;
        private Action<bool> onToggle;
        private Action whenDropdown;
        private Action onContentAdded;


        internal static void CreateToggleCardsCategoryMenu(GameObject parent, string categoryPath, string modPrefix) {
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
                    GameObject menuInstance = GameObject.Instantiate(Prefab);
                    menuInstance.transform.SetParent(currentParent.transform, false);

                    var newMenu = menuInstance.GetComponent<ToggleCardsCategoryMenu>();
                    newMenu.categoryText.text = segment;
                    newMenu.categoryDepth = i;

                    Menus.Add($"{modPrefix}/{fullPath}", newMenu);

                    if(previousMenu) {
                        previousMenu.categories.Add(newMenu);
                        previousMenu.categoriesContent.SetActive(true);
                    }

                    currentParent = newMenu.categoriesContent;
                    previousMenu = newMenu;
                }
            }
        }

        internal static ToggleCardsCategoryMenu AddToggleCardToCategory(GameObject parent, GameObject toggleCard, string categoryPath, string modPrefix) {
            if(!Menus.ContainsKey($"{modPrefix}/{categoryPath}")) CreateToggleCardsCategoryMenu(parent, categoryPath, modPrefix);
            var targetMenu = Menus[$"{modPrefix}/{categoryPath}"];

            toggleCard.transform.SetParent(targetMenu.cardsContent.transform, false);
            targetMenu.categoryContent.Add(toggleCard);
            targetMenu.categoryCardsContent.Add(toggleCard);
            targetMenu.cardsInfo.Add(CardManager.GetCardInfoWithName(toggleCard.name));
            targetMenu.onContentAdded?.Invoke();
            return targetMenu;
        }


        public void DisableCategory() {
            darkenEffect.SetActive(true);
            onToggle?.Invoke(false);

            CardManager.DisableCards(cardsInfo.ToArray());
            foreach(var toggleCards in categoryCardsContent) {
                ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
            }
        }

        public void EnableCategory() {
            darkenEffect.SetActive(false);
            onToggle?.Invoke(false);

            CardManager.EnableCards(cardsInfo.ToArray());
            foreach(var toggleCards in categoryCardsContent) {
                ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
            }
        }

        public void ToggleCategory(bool toggle) {
            darkenEffect.SetActive(!toggle);
            onToggle?.Invoke(toggle);

            if(toggle) CardManager.EnableCards(cardsInfo.ToArray());
            else CardManager.DisableCards(cardsInfo.ToArray());

            foreach(var toggleCards in categoryCardsContent) {
                ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
            }

            foreach(var category in categories) {
                if(toggle) category.EnableCategory();
                else category.DisableCategory();

                category.UpdateVisual();
            }
        }

        public void ToggleDropdown() {
            if(viewport.activeSelf) {
                viewport.SetActive(false);
                dropdownButton.image.sprite = dropdownCloseImage;
            } else {
                viewport.SetActive(true);
                dropdownButton.image.sprite = dropdownOpenImage;
                
                foreach(var toggleCards in categoryCardsContent) {
                    ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
                }
            }
            whenDropdown?.Invoke();
        }

        public void SetGridSize(int amount) {
            if(gridLayoutGroup == null) return;

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

            float depthFactor = Mathf.Clamp01(1f - 0.015f * (categoryDepth + 1));
            gridLayoutGroup.cellSize = cellSize * depthFactor;
            gridLayoutGroup.constraintCount = amount;

            foreach(var cardContent in categoryCardsContent) {
                cardContent.transform.GetChild(2).localScale = (localScale * Vector3.one * 10) * depthFactor;
            }
        }

        public void UpdateVisual() {
            bool allCardsDisabled = isDisable;

            toggleCardsButton.onValueChanged.RemoveAllListeners();
            toggleCardsButton.isOn = !allCardsDisabled;
            toggleCardsButton.onValueChanged.AddListener(ToggleCategory);

            if(allCardsDisabled) darkenEffect.SetActive(true);
            else darkenEffect.SetActive(false);
        }

        private void OnEnable() {
            viewport.SetActive(false);
            dropdownButton.image.sprite = dropdownCloseImage;

            UpdateVisual();
        }
    }
}
