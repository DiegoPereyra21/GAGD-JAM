using System;
using System.Collections.Generic;
using UnityEngine;
//para q spawneen lo que sea prefab en un area cuadrada, la idea es usar varias areas para dar realismo
namespace Game.Collectibles
{
    [Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Min(0)] public int amount = 5;
    }

    public class CollectibleSpawner : MonoBehaviour
    {
        [SerializeField] private Vector2 areaSize = new Vector2(20f, 20f);
        [SerializeField] private float minSpacing = 2f;//obligatorio para q no spawneen pegados
        private int maxAttemptsPerItem = 30; 
        [SerializeField] private List<SpawnEntry> entries = new List<SpawnEntry>();//q prefabs spawnean y cuantos
        private readonly List<Vector3> spawnedPositions = new List<Vector3>();

        private void Start()
        {
            foreach (var entry in entries)
            {
                if (entry.prefab == null) continue;

                for (int i = 0; i < entry.amount; i++)
                {
                    if (TryGetValidPosition(out Vector3 pos))
                    {
                        spawnedPositions.Add(pos);
                        Instantiate(entry.prefab, pos, Quaternion.identity, transform);
                    }
                }
            }
        }
        //genera una posicion al azar y verifica si puede colocar un objeto ahi, en caso de q no pueda, repite lo mismo 30 veces
        private bool TryGetValidPosition(out Vector3 result)
        {
            for (int attempt = 0; attempt < maxAttemptsPerItem; attempt++)
            {
                Vector3 candidate = GetRandomPointInArea();
                bool valid = true;

                foreach (var pos in spawnedPositions)
                {
                    if (Vector3.Distance(candidate, pos) < minSpacing)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    result = candidate;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }

        private Vector3 GetRandomPointInArea()
        {
            float x = UnityEngine.Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
            float z = UnityEngine.Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
            return transform.position + new Vector3(x, 0.5f, z);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.1f, areaSize.y));
        }
    }
}