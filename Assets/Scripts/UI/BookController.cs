using UnityEngine;
using UnityEngine.UIElements;

public class BookController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PotionRecipeDatabase recipeDatabase;

    private UIDocument uiDocument;
    private VisualElement root;

    private Button recipesTab;
    private Button compendiumTab;
    private Button objectivesTab;

    private Button closeButton;

    private VisualElement page;
    private VisualElement recipeBody;
    private VisualElement recipeInfo;
    private VisualElement ingredients;

    private Label title;
    private Label recipeTitle;
    private Label recipeDescription;
    private Label ingredientsTitle;

    private Button previousButton;
    private Button nextButton;
    private Label pageNumber;

    private int currentRecipeIndex = 0;

    private enum BookSection
    {
        Recipes,
        Compendium,
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

        ShowSection(BookSection.Recipes);
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }

    private void FindElements()
    {
        recipesTab = root.Q<Button>("RecipesTab");
        compendiumTab = root.Q<Button>("CompendiumTab");
        objectivesTab = root.Q<Button>("ObjectivesTab");

        closeButton = root.Q<Button>("CloseButton");

        page = root.Q<VisualElement>("Page");
        recipeBody = root.Q<VisualElement>("RecipeBody");
        recipeInfo = root.Q<VisualElement>("RecipeInfo");
        ingredients = root.Q<VisualElement>("Ingredients");

        title = root.Q<Label>("Title");
        recipeTitle = root.Q<Label>("RecipeTitle");
        recipeDescription = root.Q<Label>("RecipeDescription");
        ingredientsTitle = root.Q<Label>("IngredientsTitle");

        previousButton = root.Q<Button>("PreviousButton");
        nextButton = root.Q<Button>("NextButton");
        pageNumber = root.Q<Label>("PageNumber");
    }

    private void RegisterCallbacks()
    {
        recipesTab?.RegisterCallback<ClickEvent>(OnRecipesClicked);
        compendiumTab?.RegisterCallback<ClickEvent>(OnCompendiumClicked);
        objectivesTab?.RegisterCallback<ClickEvent>(OnObjectivesClicked);

        closeButton?.RegisterCallback<ClickEvent>(OnCloseClicked);

        previousButton?.RegisterCallback<ClickEvent>(OnPreviousClicked);
        nextButton?.RegisterCallback<ClickEvent>(OnNextClicked);
    }

    private void UnregisterCallbacks()
    {
        recipesTab?.UnregisterCallback<ClickEvent>(OnRecipesClicked);
        compendiumTab?.UnregisterCallback<ClickEvent>(OnCompendiumClicked);
        objectivesTab?.UnregisterCallback<ClickEvent>(OnObjectivesClicked);

        closeButton?.UnregisterCallback<ClickEvent>(OnCloseClicked);

        previousButton?.UnregisterCallback<ClickEvent>(OnPreviousClicked);
        nextButton?.UnregisterCallback<ClickEvent>(OnNextClicked);
    }

    private void OnRecipesClicked(ClickEvent evt)
    {
        currentRecipeIndex = 0;
        ShowSection(BookSection.Recipes);
    }

    private void OnCompendiumClicked(ClickEvent evt)
    {
        Debug.Log("CLICK COMPENDIO");

        ShowSection(BookSection.Compendium);
    }

    private void OnObjectivesClicked(ClickEvent evt)
    {
        Debug.Log("CLICK OBJETIVOS");

        ShowSection(BookSection.Objectives);
    }

    private void OnCloseClicked(ClickEvent evt)
    {
        CloseBook();
    }

    private void OnPreviousClicked(ClickEvent evt)
    {
        if (currentSection != BookSection.Recipes)
            return;

        if (recipeDatabase == null)
            return;

        if (recipeDatabase.Recipes.Count == 0)
            return;

        currentRecipeIndex--;

        if (currentRecipeIndex < 0)
            currentRecipeIndex = recipeDatabase.Recipes.Count - 1;

        DisplayCurrentRecipe();
    }

    private void OnNextClicked(ClickEvent evt)
    {
        if (currentSection != BookSection.Recipes)
            return;

        if (recipeDatabase == null)
            return;

        if (recipeDatabase.Recipes.Count == 0)
            return;

        currentRecipeIndex++;

        if (currentRecipeIndex >= recipeDatabase.Recipes.Count)
            currentRecipeIndex = 0;

        DisplayCurrentRecipe();
    }

    private void ShowSection(BookSection section)
    {
        currentSection = section;

        switch (section)
        {
            case BookSection.Recipes:
                ShowRecipes();
                break;

            case BookSection.Compendium:
                ShowCompendium();
                break;

            case BookSection.Objectives:
                ShowObjectives();
                break;
        }
    }

    private void ShowRecipes()
    {
        if (title != null)
            title.text = "Cuaderno";

        if (recipeDatabase == null)
        {
            Debug.LogError("BookController: Recipe Database no está asignado.");

            ShowEmptyRecipePage("No hay recetas disponibles.");
            return;
        }

        if (recipeDatabase.Recipes == null || recipeDatabase.Recipes.Count == 0)
        {
            Debug.LogWarning("BookController: El Recipe Database no contiene recetas.");

            ShowEmptyRecipePage("No hay recetas disponibles.");
            return;
        }

        if (currentRecipeIndex >= recipeDatabase.Recipes.Count)
            currentRecipeIndex = 0;

        DisplayCurrentRecipe();
    }

    private void DisplayCurrentRecipe()
{
    PotionRecipe recipe = recipeDatabase.Recipes[currentRecipeIndex];

    if (recipe == null)
    {
        ShowEmptyRecipePage("Receta no disponible.");
        return;
    }

    if (recipeTitle != null)
        recipeTitle.text = recipe.potionName;

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

        ingredientLabel.text = $"{ingredientName} x{amount}";

        ingredientLabel.AddToClassList("ingredient-entry");

        ingredients.Add(ingredientLabel);
    }

    private void ClearIngredients()
    {
        if (ingredients == null)
            return;

        ingredients.Clear();
    }

    private void UpdatePageNumber()
    {
        if (pageNumber == null)
            return;

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

    private void UpdateNavigationButtons()
    {
        bool hasRecipes =
            recipeDatabase != null &&
            recipeDatabase.Recipes != null &&
            recipeDatabase.Recipes.Count > 0;

        if (previousButton != null)
            previousButton.SetEnabled(hasRecipes);

        if (nextButton != null)
            nextButton.SetEnabled(hasRecipes);
    }

    private void ShowEmptyRecipePage(string message)
    {
        if (recipeTitle != null)
            recipeTitle.text = message;

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
        Debug.Log("MOSTRANDO COMPENDIO");

        if (recipeTitle != null)
            recipeTitle.text = "Compendio";

        if (recipeDescription != null)
        {
            recipeDescription.text = "";
            recipeDescription.style.display = DisplayStyle.None;
        }

        if (ingredientsTitle != null)
            ingredientsTitle.text = "";

        ClearIngredients();

        if (pageNumber != null)
            pageNumber.text = "";

        if (previousButton != null)
            previousButton.SetEnabled(false);

        if (nextButton != null)
            nextButton.SetEnabled(false);
    }

    private void ShowObjectives()
    {
        Debug.Log("MOSTRANDO OBJETIVOS");

        if (recipeTitle != null)
            recipeTitle.text = "Objetivos";

        if (recipeDescription != null)
        {
            recipeDescription.text = "";
            recipeDescription.style.display = DisplayStyle.None;
        }

        if (ingredientsTitle != null)
            ingredientsTitle.text = "";

        ClearIngredients();

        if (pageNumber != null)
            pageNumber.text = "1 / 1";

        if (previousButton != null)
            previousButton.SetEnabled(false);

        if (nextButton != null)
            nextButton.SetEnabled(false);
    }

    public void OpenBook()
    {
        if (root == null)
            return;

        root.style.display = DisplayStyle.Flex;

        currentRecipeIndex = 0;
        ShowSection(BookSection.Recipes);
    }

    public void CloseBook()
    {
        if (root == null)
            return;

        root.style.display = DisplayStyle.None;
    }
}