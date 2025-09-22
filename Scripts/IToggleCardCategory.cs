using System;

namespace ToggleCardsCategories {
    public struct ToggleCardCategoryInfo {
        public string Name;
        public Nullable<int> Priority;

        public ToggleCardCategoryInfo(string name, Nullable<int> priority = null) {
            Name = name;
            Priority = priority;
        }
    }


    internal interface IToggleCardCategory {
        ToggleCardCategoryInfo GetCardCategoryInfo();
    }
}
