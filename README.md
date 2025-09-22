# Toggle Cards Categories

The **Toggle Cards Categories** allow you to create categories in your toggle mod category, Scroll down below for how use this library.

# Preview
The screenshot below shows categories in the **AAC** mod with the **Exo Armor** category expanded.  
This gives you an idea of how your own categories will look. <img width="1329" height="963" alt="Screenshot 2025-09-22 003706" src="https://github.com/user-attachments/assets/41d1744d-ba81-4e6d-b9d0-c512030de45d" />


# Getting Started (Visual Studio)
After you added the mod as dependency, you need to create a toggle categories for your mods, first you must register your mod initials to the `ToggleCardsCategoriesManager`, you can do this by writing this line of code in your mod  awake method
```Cs
ToggleCardsCategoriesManager.instance.RegisterCategories(MyModInitials);
```
This will make the mod create categories in that toggle mod section

The mod will look for the interface `IToggleCardCategory` for each of your non hidden cards, So if you want put a specific card in a specific category you must implement the `IToggleCardCategory` for your custom card like this
```cs
using ToggleCardsCategories;

public class MyCustomCard : CustomCard {
	public ToggleCardCategoryInfo GetCardCategoryInfo() {
		return new ToggleCardCategoryInfo("MyCategoryName")
	}
	// Rest the code below
}
```
TIP: The seconds parameter for the `ToggleCardCategoryInfo` struct is the category priority

After you does that on all your cards you should be able to build your mods, and see your cards have be put in categories
# Getting Started (Unity)
After you have imported this library into your unity project, you need to create a toggle categories for your mods, first you must register your mod initials to the `ToggleCardsCategoriesManager`, you can do this by writing this line of code in your mod awake method
```cs
ToggleCardsCategoriesManager.instance.RegisterCategories(MyModInitials);
```
This will make the mod create categories in that toggle mod section

The mod will look for the interface `IToggleCardCategory` for each of your non hidden cards, So if you want put a specific card in a specific category you must the implement the `IToggleCardCategory` or since you in **Unity** you can add the the `AddToToggleCardCategory` component, and set the `CategoryPath` in the component

After you does that on all your cards you should be able to build your mods, and see your cards have be put in categories

# Extra Tips
The the `ToggleCardsCategoryMenu` have a static dictionary of category and the `ToggleCardsCategoryMenu` instance called `Menus` , the `ToggleCardsCategoryMenu` instance also have the `Priority` property, there is some example code:
```cs
void Start() {
	this.ExecuteAfterSeconds(1, () => {
		if(ToggleCardsCategoryMenu.Menus.ContainsKey("MyModInitials/Classes")) {
			ToggleCardsCategoryMenu.Menus["MyModInitials/Classes"].Priority = 10;
		}
	})
}
```
