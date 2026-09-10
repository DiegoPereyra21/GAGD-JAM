using System;
using System.Collections;
using UnityEngine;
//todo sobre la logica del objeto agarrable, y su mini aniumacion de recolectado
namespace Game.Collectibles
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private int value = 1;
        //esta bastante bien asi, no creo q haga falta tocarlo
        [SerializeField] private float collectDuration = 0.3f;
        [SerializeField] private float collectRiseHeight = 1f;
        //para hacer el menu interactivo dentro del cesto
        [SerializeField] private GameObject basketVisualPrefab;
        public GameObject BasketVisualPrefab => basketVisualPrefab;

        [SerializeField] private IngredientType type;
        public IngredientType Type => type;
        //para q cambie de modelo luego de recolectado, es el caso de los arboles y arbustos
        public event Action OnCollected;

        //para que al quitar un arbol, arbusto y tronco no se los "chupe", sino que los sacuda
        [SerializeField] private bool destroyOnCollect = true;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeStrength = 0.1f;
        
        //para q no me congele al recolectar objetos q ya no puedo recolectar
        [SerializeField] private float freezeDuration = 0.8f;
        public float FreezeDuration => freezeDuration;
        public bool IsCollected => collected;

        private RandomVisualVariant visualVariant;

        //para el outline al acercarse
        [SerializeField] private InteractableOutline outline;
        public void SetHighlighted(bool highlighted)
        {
            if (collected) return;
            outline?.SetHighlighted(highlighted);
        }

        public int Value => value;
        private bool collected;
        


        [SerializeField] private float dayShrinkDuration = 1.5f;
        [SerializeField] private float dayShrinkScaleFactor = 0.35f;
        private bool shrunk;

        private void OnEnable()
        {
            GameProgressManager.Instance.OnNightTimeExpired += HandleDayBroke;
            GameProgressManager.Instance.OnDayStarted += HandleDayBroke;
        }

        private void OnDisable()
        {
            GameProgressManager.Instance.OnNightTimeExpired -= HandleDayBroke;
            GameProgressManager.Instance.OnDayStarted -= HandleDayBroke;
        }


        private void Awake()
        {
            visualVariant = GetComponent<RandomVisualVariant>();
        }
        
        private void HandleDayBroke()
        {
            if (collected || shrunk) return;
            shrunk = true;

            if (visualVariant != null)
            {
                visualVariant.ShowDepletedLook();
                return;
            }

            StartCoroutine(ShrinkRoutine());
        }

        private IEnumerator ShrinkRoutine()
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = startScale * dayShrinkScaleFactor;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            float baseY = transform.position.y;
            if (renderers.Length > 0)
            {
                float minY = float.MaxValue;
                foreach (Renderer r in renderers)
                    minY = Mathf.Min(minY, r.bounds.min.y);
                baseY = minY;
            }

            float pivotOffset = transform.position.y - baseY;
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < dayShrinkDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dayShrinkDuration;

                Vector3 currentScale = Vector3.Lerp(startScale, targetScale, t);
                transform.localScale = currentScale;

                float scaleRatio = currentScale.y / startScale.y;
                transform.position = new Vector3(startPos.x, baseY + pivotOffset * scaleRatio, startPos.z);

                yield return null;
            }

            transform.localScale = targetScale;
            transform.position = new Vector3(startPos.x, baseY + pivotOffset * dayShrinkScaleFactor, startPos.z);
        }



        public void Collect(Action onComplete = null)
        {
            if (collected) return;
            collected = true;
            outline?.SetHighlighted(false);

            if (destroyOnCollect)
                StartCoroutine(CollectRoutine(onComplete));
            else
                StartCoroutine(ShakeRoutine(onComplete));
        }

        private IEnumerator CollectRoutine(Action onComplete)
        {
            Vector3 startPos = transform.position;
            Vector3 startScale = transform.localScale;
            float t = 0f;

            while (t < collectDuration)
            {
                t += Time.deltaTime;
                float p = t / collectDuration;
                transform.position = startPos + Vector3.up * collectRiseHeight * p;
                transform.localScale = startScale * (1f - p);
                yield return null;
            }

            onComplete?.Invoke();
            OnCollected?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator ShakeRoutine(Action onComplete)
        {
            Vector3 originalPos = transform.position;
            float t = 0f;

            while (t < shakeDuration)
            {
                t += Time.deltaTime;
                float offsetX = UnityEngine.Random.Range(-shakeStrength, shakeStrength);
                float offsetZ = UnityEngine.Random.Range(-shakeStrength, shakeStrength);
                transform.position = originalPos + new Vector3(offsetX, 0f, offsetZ);
                yield return null;
            }

            transform.position = originalPos;
            onComplete?.Invoke();
            OnCollected?.Invoke();
        }
    }
}