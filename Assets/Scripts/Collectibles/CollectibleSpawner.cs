using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Collectibles
{
    [Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        [Min(0)] public int amount = 5;
        public float groundOffset = 0f;
    }

    public class CollectibleSpawner : MonoBehaviour
    {
        [SerializeField] private Vector2 areaSize = new Vector2(20f, 20f);
        [SerializeField] private float minSpacing = 2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float raycastHeight = 50f;
        [SerializeField] private bool alignToTerrainNormal = false;
        private int maxAttemptsPerItem = 30;
        [SerializeField] private List<SpawnEntry> entries = new List<SpawnEntry>();
        private readonly List<Vector3> spawnedPositions = new List<Vector3>();

        private void Start()
        {
            foreach (var entry in entries)
            {
                if (entry.prefab == null) continue;

                for (int i = 0; i < entry.amount; i++)
                {
                    if (TryGetValidPosition(out Vector3 pos, out Vector3 normal))
                    {
                        spawnedPositions.Add(pos);

                        Vector3 spawnPos = pos + Vector3.up * entry.groundOffset;

                        Quaternion rotation = alignToTerrainNormal
                            ? Quaternion.FromToRotation(Vector3.up, normal)
                            : Quaternion.identity;

                        Instantiate(entry.prefab, spawnPos, rotation, transform);
                    }
                }
            }
        }

        private bool TryGetValidPosition(out Vector3 result, out Vector3 normal)
        {
            for (int attempt = 0; attempt < maxAttemptsPerItem; attempt++)
            {
                if (!TryGetGroundPoint(out Vector3 candidate, out Vector3 candidateNormal)) continue;

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
                    normal = candidateNormal;
                    return true;
                }
            }

            result = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        private bool TryGetGroundPoint(out Vector3 point, out Vector3 normal)
        {
            float x = UnityEngine.Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
            float z = UnityEngine.Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
            Vector3 origin = transform.position + new Vector3(x, raycastHeight, z);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                point = hit.point;
                normal = hit.normal;
                return true;
            }

            point = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.1f, areaSize.y));
        }
    }
}