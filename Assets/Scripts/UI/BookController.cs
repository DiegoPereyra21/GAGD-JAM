using Game.Collectibles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BookController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PotionRecipeDatabase recipeDatabase;

    [Header("Input")]
    [SerializeField] private InputActionReference openBookAction;

    [Header("Ingredientes")]
    [SerializeField] private IngredientDatabase ingredientDatabase;

    private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement bookContainer;


    // tabs
    private Button startTab;
    private Button recipesTab;
    private Button compendiumTab;
    private Button mapTab;
    private Button objectivesTab;


    // paginas
    private VisualElement startPage;
    private VisualElement recipesPage;
    private VisualElement compendiumPage;
    private VisualElement mapPage;
    private VisualElement objectivesPage;


    // header
    private Button closeButton;
    private Label title;


    //inicio
    private Label startPageTitle;
    private Label descriptionLabel;


    // recetas
    private Label recipeTitle;
    private Image recipeImage;
    private Label recipeDescription;
    private Label ingredientsTitle;
    private VisualElement ingredients;


    //compendio
    private Label compendiumTitle;
    private Image compendiumImage;
    private Label compendiumDescription;


    //navegacion
    private VisualElement pageNavigation;
    private Button previousButton;
    private Button nextButton;
    private Label pageNumber;


    //estado
    private bool isBookOpen = false;

    private int currentRecipeIndex = 0;
    private int currentIngredientIndex = 0;


    private enum BookSection
    {
        Start,
        Recipes,
        Compendium,
        Map,
        Objectives
    }

    private BookSection currentSection;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }


    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        FindElements();
        RegisterCallbacks();

        if (bookContainer != null)
            bookContainer.style.display = DisplayStyle.None;

        isBookOpen = false;

        if (openBookAction != null)
        {
            openBookAction.action.Enable();
            openBookAction.action.performed += OnOpenBookPerformed;
        }
    }


    private void OnDisable()
    {
        UnregisterCallbacks();

        if (openBookAction != null)
        {
            openBookAction.action.performed -= OnOpenBookPerformed;
            openBookAction.action.Disable();
        }
    }

    private void FindElements()
    {
        bookContainer = root.Q<VisualElement>("BookContainer");

        startTab = root.Q<Button>("StartTab");
        recipesTab = root.Q<Button>("RecipesTab");
        compendiumTab = root.Q<Button>("CompendiumTab");
        mapTab = root.Q<Button>("MapTab");
        objectivesTab = root.Q<Button>("ObjectivesTab");

        startPage = root.Q<VisualElement>("StartPage");
        recipesPage = root.Q<VisualElement>("RecipesPage");
        compendiumPage = root.Q<VisualElement>("CompendiumPage");
        mapPage = root.Q<VisualElement>("MapPage");
        objectivesPage = root.Q<VisualElement>("ObjectivesPage");

        closeButton = root.Q<Button>("CloseButton");
        title = root.Q<Label>("Title");

        startPageTitle = root.Q<Label>("StartPageTitle");
        descriptionLabel = root.Q<Label>("DescriptionLabel");

        recipeTitle = root.Q<Label>("RecipeTitle");
        recipeImage = root.Q<Image>("RecipeImage");
        recipeDescription = root.Q<Label>("RecipeDescription");
        ingredientsTitle = root.Q<Label>("IngredientsTitle");
        ingredients = root.Q<VisualElement>("Ingredients");

        compendiumTitle = root.Q<Label>("CompendiumTitle");
        compendiumImage = root.Q<Image>("CompendiumImage");
        compendiumDescription = root.Q<Label>("CompendiumDescription");

        pageNavigation = root.Q<VisualElement>("PageNavigation");
        previousButton = root.Q<Button>("PreviousButton");
        nextButton = root.Q<Button>("NextButton");
        pageNumber = root.Q<Label>("PageNumber");
    }

    private void RegisterCallbacks()
    {
        startTab?.RegisterCallback<ClickEvent>(OnStartClicked);
        recipesTab?.RegisterCallback<ClickEvent>(OnRecipesClicked);
        compendiumTab?.RegisterCallback<ClickEvent>(OnCompendiumClicked);
        mapTab?.RegisterCallback<ClickEvent>(OnMapClicked);
        objectivesTab?.RegisterCallback<ClickEvent>(OnObjectivesClicked);

        closeButton?.RegisterCallback<ClickEvent>(OnCloseClicked);

        previousButton?.RegisterCallback<ClickEvent>(OnPreviousClicked);
        nextButton?.RegisterCallback<ClickEvent>(OnNextClicked);
    }


    private void UnregisterCallbacks()
    {
        startTab?.UnregisterCallback<ClickEvent>(OnStartClicked);
        recipesTab?.UnregisterCallback<ClickEvent>(OnRecipesClicked);
        compendiumTab?.UnregisterCallback<ClickEvent>(OnCompendiumClicked);
        mapTab?.UnregisterCallback<ClickEvent>(OnMapClicked);
        objectivesTab?.UnregisterCallback<ClickEvent>(OnObjectivesClicked);

        closeButton?.UnregisterCallback<ClickEvent>(OnCloseClicked);

        previousButton?.UnregisterCallback<ClickEvent>(OnPreviousClicked);
        nextButton?.UnregisterCallback<ClickEvent>(OnNextClicked);
    }

    private void OnOpenBookPerformed(InputAction.CallbackContext context)
    {
        if (isBookOpen)
            CloseBook();
        else
            OpenBook();
    }

    private void OnStartClicked(ClickEvent evt)
    {
        ShowSection(BookSection.Start);
    }


    private void OnRecipesClicked(ClickEvent evt)
    {
        currentRecipeIndex = 0;

        ShowSection(BookSection.Recipes);
    }


    private void OnCompendiumClicked(ClickEvent evt)
    {
        currentIngredientIndex = 0;

        ShowSection(BookSection.Compendium);
    }


    private void OnMapClicked(ClickEvent evt)
    {
        ShowSection(BookSection.Map);
    }


    private void OnObjectivesClicked(ClickEvent evt)
    {
        ShowSection(BookSection.Objectives);
    }


    private void OnCloseClicked(ClickEvent evt)
    {
        CloseBook();
    }

    private void ShowSection(BookSection section)
    {
        currentSection = section;

        HideAllPages();

        switch (section)
        {
            case BookSection.Start:
                ShowStart();
                break;

            case BookSection.Recipes:
                ShowRecipes();
                break;

            case BookSection.Compendium:
                ShowCompendium();
                break;

            case BookSection.Map:
                ShowMap();
                break;

            case BookSection.Objectives:
                ShowObjectives();
                break;
        }

        UpdateNavigationVisibility();
    }


    private void HideAllPages()
    {
        if (startPage != null)
            startPage.style.display = DisplayStyle.None;

        if (recipesPage != null)
            recipesPage.style.display = DisplayStyle.None;

        if (compendiumPage != null)
            compendiumPage.style.display = DisplayStyle.None;

        if (mapPage != null)
            mapPage.style.display = DisplayStyle.None;

        if (objectivesPage != null)
            objectivesPage.style.display = DisplayStyle.None;
    }

    private void ShowStart()
    {
        if (startPage != null)
            startPage.style.display = DisplayStyle.Flex;

        if (title != null)
            title.text = "GRIMORIO";

        if (startPageTitle != null)
            startPageTitle.text = "Inicio";
    }


    private void ShowRecipes()
    {
        if (recipesPage != null)
            recipesPage.style.display = DisplayStyle.Flex;

        if (title != null)
            title.text = "GRIMORIO";

        if (recipeDatabase == null)
        {
            ShowEmptyRecipePage("No hay recetas disponibles.");
            return;
        }

        if (recipeDatabase.Recipes == null ||
            recipeDatabase.Recipes.Count == 0)
        {
            ShowEmptyRecipePage("No hay recetas disponibles.");
            return;
        }

        if (currentRecipeIndex >= recipeDatabase.Recipes.Count)
            currentRecipeIndex = 0;

        DisplayCurrentRecipe();
    }


    private void DisplayCurrentRecipe()
    {
        PotionRecipe recipe =
            recipeDatabase.Recipes[currentRecipeIndex];

        if (recipe == null)
        {
            ShowEmptyRecipePage("Receta no disponible.");
            return;
        }

        if (recipeTitle != null)
            recipeTitle.text = recipe.potionName;


        if (recipeImage != null)
        {
            recipeImage.sprite = recipe.image2D;
            recipeImage.style.display = DisplayStyle.Flex;
        }

        if (recipeDescription != null)
        {
            recipeDescription.text = recipe.description;
            recipeDescription.style.display = DisplayStyle.Flex;
        }

        if (ingredientsTitle != null)
            ingredientsTitle.text = "Ingredientes";

        ClearIngredients();

        if (recipe.ingredients != null)
        {
            foreach (RecipeIngredient ingredient in recipe.ingredients)
            {
                if (ingredient == null || ingredient.type == null)
                    continue;

                AddIngredientLine(
                    ingredient.type.displayName,
                    ingredient.amount
                );
            }
        }

        UpdatePageNumber();
        UpdateNavigationButtons();
    }


    private void AddIngredientLine(string ingredientName, int amount)
    {
        if (ingredients == null)
            return;

        Label ingredientLabel = new Label();

        ingredientLabel.text =
            $"{ingredientName} x{amount}";

        ingredientLabel.AddToClassList("ingredient-entry");

        ingredients.Add(ingredientLabel);
    }


    private void ClearIngredients()
    {
        if (ingredients == null)
            return;

        ingredients.Clear();
    }


    private void ShowEmptyRecipePage(string message)
    {
        if (recipeTitle != null)
            recipeTitle.text = message;


        if (recipeImage != null)
        {
            recipeImage.sprite = null;
            recipeImage.style.display = DisplayStyle.None;
        }


        if (recipeDescription != null)
        {
            recipeDescription.text = "";
            recipeDescription.style.display = DisplayStyle.None;
        }


        if (ingredientsTitle != null)
            ingredientsTitle.text = "";

        ClearIngredients();


        if (pageNumber != null)
            pageNumber.text = "0 / 0";

        if (previousButton != null)
            previousButton.SetEnabled(false);

        if (nextButton != null)
            nextButton.SetEnabled(false);
    }

    private void ShowCompendium()
    {
        if (compendiumPage != null)
            compendiumPage.style.display = DisplayStyle.Flex;

        if (title != null)
            title.text = "GRIMORIO";

        if (ingredientDatabase == null ||
            ingredientDatabase.AllIngredients == null ||
            ingredientDatabase.AllIngredients.Count == 0)
        {
            ShowEmptyCompendiumPage();
            return;
        }

        if (currentIngredientIndex >= ingredientDatabase.AllIngredients.Count)
            currentIngredientIndex = 0;

        DisplayCurrentIngredient();
    }

    private void DisplayCurrentIngredient()
    {
        IngredientType ingredient =
            ingredientDatabase.AllIngredients[currentIngredientIndex];

        if (ingredient == null)
        {
            ShowEmptyCompendiumPage();
            return;
        }

        if (compendiumTitle != null)
            compendiumTitle.text = ingredient.displayName;

        if (compendiumImage != null)
        {
            compendiumImage.sprite = ingredient.image;
            compendiumImage.style.display = DisplayStyle.Flex;
        }

        if (compendiumDescription != null)
        {
            compendiumDescription.text = ingredient.locationDescription;
            compendiumDescription.style.display = DisplayStyle.Flex;
        }

        UpdatePageNumber();
        UpdateNavigationButtons();
    }

    private void ShowEmptyCompendiumPage()
    {
        if (compendiumTitle != null)
            compendiumTitle.text = "No hay ingredientes disponibles.";

        if (compendiumImage != null)
        {
            compendiumImage.sprite = null;
            compendiumImage.style.display = DisplayStyle.None;
        }

        if (compendiumDescription != null)
        {
            compendiumDescription.text = "";
            compendiumDescription.style.display = DisplayStyle.None;
        }

        if (pageNumber != null)
            pageNumber.text = "0 / 0";

        if (previousButton != null)
            previousButton.SetEnabled(false);

        if (nextButton != null)
            nextButton.SetEnabled(false);
    }

    private void ShowMap()
    {
        if (mapPage != null)
            mapPage.style.display = DisplayStyle.Flex;

        if (title != null)
            title.text = "GRIMORIO";
    }

    private void ShowObjectives()
    {
        if (objectivesPage != null)
            objectivesPage.style.display = DisplayStyle.Flex;

        if (title != null)
            title.text = "GRIMORIO";
    }

    private void UpdateNavigationVisibility()
    {
        bool showNavigation =
            currentSection == BookSection.Recipes ||
            currentSection == BookSection.Compendium;

        if (pageNavigation != null)
        {
            pageNavigation.style.display =
                showNavigation
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        if (!showNavigation)
        {
            if (previousButton != null)
                previousButton.SetEnabled(false);

            if (nextButton != null)
                nextButton.SetEnabled(false);

            if (pageNumber != null)
                pageNumber.text = "";
        }
    }


    private void UpdateNavigationButtons()
    {
        bool hasPages = false;

        if (currentSection == BookSection.Recipes)
        {
            hasPages =
                recipeDatabase != null &&
                recipeDatabase.Recipes != null &&
                recipeDatabase.Recipes.Count > 0;
        }
        else if (currentSection == BookSection.Compendium)
        {
            hasPages =
                ingredientDatabase != null &&
                ingredientDatabase.AllIngredients != null &&
                ingredientDatabase.AllIngredients.Count > 0;
        }

        if (previousButton != null)
            previousButton.SetEnabled(hasPages);

        if (nextButton != null)
            nextButton.SetEnabled(hasPages);
    }


    private void UpdatePageNumber()
    {
        if (pageNumber == null)
            return;

        if (currentSection == BookSection.Recipes)
        {
            if (recipeDatabase == null ||
                recipeDatabase.Recipes == null ||
                recipeDatabase.Recipes.Count == 0)
            {
                pageNumber.text = "0 / 0";
                return;
            }

            pageNumber.text =
                $"{currentRecipeIndex + 1} / {recipeDatabase.Recipes.Count}";
        }
        else if (currentSection == BookSection.Compendium)
        {
            if (ingredientDatabase == null ||
                ingredientDatabase.AllIngredients == null ||
                ingredientDatabase.AllIngredients.Count == 0)
            {
                pageNumber.text = "0 / 0";
                return;
            }

            pageNumber.text =
                $"{currentIngredientIndex + 1} / {ingredientDatabase.AllIngredients.Count}";
        }
    }


    private void OnPreviousClicked(ClickEvent evt)
    {
        if (currentSection == BookSection.Recipes)
        {
            if (recipeDatabase == null ||
                recipeDatabase.Recipes == null ||
                recipeDatabase.Recipes.Count == 0)
                return;

            currentRecipeIndex--;

            if (currentRecipeIndex < 0)
                currentRecipeIndex =
                    recipeDatabase.Recipes.Count - 1;

            DisplayCurrentRecipe();
        }
        else if (currentSection == BookSection.Compendium)
        {
            if (ingredientDatabase == null ||
                ingredientDatabase.AllIngredients == null ||
                ingredientDatabase.AllIngredients.Count == 0)
                return;

            currentIngredientIndex--;

            if (currentIngredientIndex < 0)
                currentIngredientIndex =
                    ingredientDatabase.AllIngredients.Count - 1;

            DisplayCurrentIngredient();
        }
    }


    private void OnNextClicked(ClickEvent evt)
    {
        if (currentSection == BookSection.Recipes)
        {
            if (recipeDatabase == null ||
                recipeDatabase.Recipes == null ||
                recipeDatabase.Recipes.Count == 0)
                return;

            currentRecipeIndex++;

            if (currentRecipeIndex >= recipeDatabase.Recipes.Count)
                currentRecipeIndex = 0;

            DisplayCurrentRecipe();
        }
        else if (currentSection == BookSection.Compendium)
        {
            if (ingredientDatabase == null ||
                ingredientDatabase.AllIngredients == null ||
                ingredientDatabase.AllIngredients.Count == 0)
                return;

            currentIngredientIndex++;

            if (currentIngredientIndex >= ingredientDatabase.AllIngredients.Count)
                currentIngredientIndex = 0;

            DisplayCurrentIngredient();
        }
    }

    public void OpenBook()
    {
        if (bookContainer == null)
            return;

        bookContainer.style.display = DisplayStyle.Flex;

        isBookOpen = true;

        currentRecipeIndex = 0;
        currentIngredientIndex = 0;

        ShowSection(BookSection.Start);
    }


    public void CloseBook()
    {
        if (bookContainer == null)
            return;

        bookContainer.style.display = DisplayStyle.None;

        isBookOpen = false;
    }
}