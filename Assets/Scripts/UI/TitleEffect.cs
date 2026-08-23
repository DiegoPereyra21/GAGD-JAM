using UnityEngine;
using UnityEngine.UIElements;

public class TitleEffect : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement title;
    private VisualElement sparkles;

    private float time;
    private float sparkleTimer;

    private void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        title = root.Q<Label>("Title");

        if (title == null)
        {
            Debug.LogError("❌ TitleEffect: no encontré el Label llamado Title.");
            return;
        }

        Debug.Log("✅ TitleEffect: encontré el título.");

        // Contenedor de sparkles
        sparkles = new VisualElement();

        sparkles.style.position = Position.Absolute;
        sparkles.style.left = 0;
        sparkles.style.top = 0;
        sparkles.style.right = 0;
        sparkles.style.bottom = 0;

        sparkles.pickingMode = PickingMode.Ignore;

        // Los sparkles pertenecen al título
        title.Add(sparkles);

        // Primer sparkle después de un pequeño intervalo
        sparkleTimer = Random.Range(1.5f, 3.5f);
    }

    private void Update()
    {
        if (title == null)
            return;

        time += Time.deltaTime;

        // =========================================
        // 🌙 MOVIMIENTO VERTICAL
        // =========================================

        float verticalMovement =
            Mathf.Sin(time * 0.65f) * 4f +
            Mathf.Sin(time * 1.17f) * 1.2f +
            Mathf.Sin(time * 0.31f) * 1.8f;

        title.style.translate = new Translate(
            Length.Pixels(0),
            Length.Pixels(verticalMovement)
        );


        // =========================================
        // 🫧 RESPIRACIÓN ORGÁNICA
        // =========================================

        float breathingWave =
            Mathf.Sin(time * 0.55f) +
            Mathf.Sin(time * 0.23f) * 0.35f;

        float breathing =
            1f + breathingWave * 0.008f;

        title.style.scale = new Scale(
            new Vector2(breathing, breathing)
        );


        // =========================================
        // ✨ SPARKLES
        // =========================================

        sparkleTimer -= Time.deltaTime;

        if (sparkleTimer <= 0f)
        {
            CreateSparkle();

            sparkleTimer = Random.Range(2f, 5f);
        }
    }

    private void CreateSparkle()
    {
        VisualElement sparkle = new VisualElement();

        sparkle.AddToClassList("title-sparkle");

        // Violeta o dorado
        bool gold = Random.value > 0.5f;

        if (gold)
        {
            sparkle.style.backgroundColor =
                new Color(1f, 0.82f, 0.35f, 0.9f);
        }
        else
        {
            sparkle.style.backgroundColor =
                new Color(0.75f, 0.45f, 1f, 0.85f);
        }

        // Tamaño aleatorio
        float size = Random.Range(2f, 5f);

        sparkle.style.width = size;
        sparkle.style.height = size;

        // Posición alrededor del título
        sparkle.style.left = Random.Range(20f, 380f);
        sparkle.style.top = Random.Range(10f, 90f);

        // Empieza invisible
        sparkle.style.opacity = 0f;

        sparkles.Add(sparkle);

        // Aparece
        sparkle.schedule.Execute(() =>
        {
            sparkle.style.opacity = 1f;

        }).StartingIn(50);

        // Desaparece
        sparkle.schedule.Execute(() =>
        {
            sparkle.style.opacity = 0f;

        }).StartingIn(350);

        // Se elimina
        sparkle.schedule.Execute(() =>
        {
            sparkle.RemoveFromHierarchy();

        }).StartingIn(650);
    }
}