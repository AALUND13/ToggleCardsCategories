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
        [SerializeField] internal Sprite dropdownCloseImage;
        [SerializeField] private Toggle toggleCardsButton;
        [SerializeField] internal Button dropdownButton;

        [SerializeField] internal GameObject viewport;
        [SerializeField] internal GameObject cardsContent;
        [SerializeField] internal GameObject categoriesContent;

        [SerializeField] private GameObject darkenEffect;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        [SerializeField] internal TextMeshProUGUI categoryText;


        internal List<CardInfo> cardsInfo = new List<CardInfo>();
        internal List<ToggleCardsCategoryMenu> categories = new List<ToggleCardsCategoryMenu>();
        internal ToggleCardsCategoryMenu parentCategory = null;

        internal List<GameObject> categoryContent = new List<GameObject>();
        internal List<GameObject> categoryCardsContent = new List<GameObject>();

        internal int categoryDepth;
        private bool LockIsDropdown;


        public void DisableCategory() {
            darkenEffect.SetActive(true);

            CardManager.DisableCards(cardsInfo.ToArray());
            foreach(var toggleCards in categoryCardsContent) {
                ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
            }
        }

        public void EnableCategory() {
            darkenEffect.SetActive(false);

            CardManager.EnableCards(cardsInfo.ToArray());
            foreach(var toggleCards in categoryCardsContent) {
                ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
            }
        }


        public void CollapseCategory() {
            if(viewport.activeSelf) {
                viewport.SetActive(false);
                dropdownButton.image.sprite = dropdownCloseImage;

                UnityEngine.Debug.Log($"{categoryText.text} have been collapse");
            }
        }

        public void ExpandCategory() {
            if(!viewport.activeSelf) {
                viewport.SetActive(true);
                dropdownButton.image.sprite = dropdownOpenImage;

                foreach(var toggleCards in categoryCardsContent) {
                    ToggleCardsMenuHandler.UpdateVisualsCardObj(toggleCards);
                }

                UnityEngine.Debug.Log($"{categoryText.text} have been expanded");
            }
        }


        public void ToggleCategory(bool toggle) {
            darkenEffect.SetActive(!toggle);

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
                CollapseCategory();
            } else {
                ExpandCategory();
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

        public void LockDropdown() {
            if(!LockIsDropdown) dropdownButton.image.color /= 2;
        }

        public void unLockDropdown() {
            if(LockIsDropdown) dropdownButton.image.color *= 2;
        }

        internal void AddToggleCardToCategory(GameObject toggleCard) {
            toggleCard.transform.SetParent(cardsContent.transform, false);
            categoryContent.Add(toggleCard);
            categoryCardsContent.Add(toggleCard);
            cardsInfo.Add(CardManager.GetCardInfoWithName(toggleCard.name));
        }

        internal void SetGridSize(int amount) {
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

        private void OnEnable() {
            UpdateVisual();
        }
    }
}
