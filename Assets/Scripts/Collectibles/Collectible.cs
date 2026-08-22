using System;
using System.Collections;
using UnityEngine;
//todo sobre la logica del objeto agarrable, y su mini aniumacion de recolectado
namespace Game.Collectibles
{
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private CollectibleType type;
        [SerializeField] private int value = 1;
        //esta bastante bien asi, no creo q haga falta tocarlo
        [SerializeField] private float collectDuration = 0.3f;
        [SerializeField] private float collectRiseHeight = 1f;
        public CollectibleType Type => type;
        public int Value => value;
        private bool collected;
        
        public void Collect(Action onComplete = null)
        {
            if (collected) return;
            collected = true;
            StartCoroutine(CollectRoutine(onComplete));
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
            Destroy(gameObject);
        }
    }
}