using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToggleCardsCategories {
    public class ToggleCardsCategoriesManager : MonoBehaviour {
        public static ToggleCardsCategoriesManager instance { get; internal set; }

        public IReadOnlyList<string> RegisteredCategories => registeredCategories.AsReadOnly();
        private List<string> registeredCategories = new List<string>();

        public void RegisterCategories(string category) {
            if(registeredCategories.Contains(category)) return;
            registeredCategories.Add(category);
        }
    }
}
