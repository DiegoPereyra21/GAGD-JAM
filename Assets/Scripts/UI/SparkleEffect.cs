using UnityEngine;
using UnityEngine.UIElements;

public class SparkleEffect : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        var buttons = root.Query<Button>().ToList();

        foreach (Button button in buttons)
        {
            VisualElement sparkles = button.Q<VisualElement>("Sparkles");

            if (sparkles == null)
            {
                continue;
            }

            RegisterButton(button, sparkles);
        }
    }

    private void RegisterButton(Button button, VisualElement sparkles)
    {
        bool hovering = false;

        button.RegisterCallback<MouseEnterEvent>(_ =>
        {
            hovering = true;

            GenerateSparkle(sparkles, () => hovering);
        });

        button.RegisterCallback<MouseLeaveEvent>(_ =>
        {
            hovering = false;
        });
    }

    private void GenerateSparkle(
        VisualElement sparkles,
        System.Func<bool> isHovering)
    {
        if (!isHovering())
            return;

        VisualElement sparkle = new VisualElement();

        sparkle.AddToClassList("sparkle");

        sparkles.Add(sparkle);

        // 20% de probabilidad de ser una partícula grande
        bool bigSparkle = Random.value < 0.2f;

        if (bigSparkle)
        {
            sparkle.style.width = Random.Range(6f, 10f);
            sparkle.style.height = Random.Range(6f, 10f);

            sparkle.style.backgroundColor =
                new Color(1f, 0.95f, 0.8f, 0.95f);
        }
        else
        {
            sparkle.style.width = Random.Range(2f, 5f);
            sparkle.style.height = Random.Range(2f, 5f);

            sparkle.style.backgroundColor =
                new Color(1f, 0.9f, 0.65f, 0.85f);
        }

        sparkle.style.left = Random.Range(0f, 400f);
        sparkle.style.top = Random.Range(0f, 100f);

        sparkle.style.opacity = 1f;

        // Fade out
        sparkle.schedule.Execute(() =>
        {
            sparkle.style.opacity = 0f;

            sparkle.schedule.Execute(() =>
            {
                sparkle.RemoveFromHierarchy();

            }).StartingIn(250);

        }).StartingIn(300);

        // Crear otra partícula
        sparkle.schedule.Execute(() =>
        {
            if (isHovering())
            {
                GenerateSparkle(sparkles, isHovering);
            }

        }).StartingIn(80);
    }
}