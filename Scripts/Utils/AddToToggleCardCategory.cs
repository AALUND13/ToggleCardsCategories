using UnityEngine;

namespace ToggleCardsCategories.Utils {
    public class AddToToggleCardCategory : MonoBehaviour, IToggleCardCategory {
        [Header("Category Path")]
        [Tooltip("The for the category for exmaple if the path is \"Classes/MyClassHere\" it will create a category called \"MyClassHere\" in \"Classes\"")]
        public string CategoryPath;

        [Header("Category Path")]
        [Tooltip("If enable allow you to use the \"Priority\" field allowing to set the category priority")]
        public bool UsePriority;
        [Tooltip("Charge the position of the category in the the toggle mod category")]
        public int Priority = 0;

        public ToggleCardCategoryInfo GetCardCategoryInfo() {
            return new ToggleCardCategoryInfo(CategoryPath, UsePriority ? (int?)Priority : null);
        }
    }
}
