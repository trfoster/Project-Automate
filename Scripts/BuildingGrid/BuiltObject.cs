using System;
using System.Collections.Generic;
using ProjectAutomate.BuildingSOs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectAutomate.BuildingGrid
{
    [Serializable]
    public class BuiltObject : SerializedMonoBehaviour
    {
        public static BuiltObject Create(Vector3 worldPos, Vector2Int pivotPos, float rotation, BuildingSO buildingSO)
        {
            Transform builtObjectTransform = Instantiate(buildingSO.prefab, worldPos, Quaternion.Euler(0f, 0f, rotation));
            BuiltObject builtObject = builtObjectTransform.GetComponent<BuiltObject>();
            builtObject.buildingSO = buildingSO;
            builtObject.pivotPos = pivotPos;
            builtObject.rotation = rotation;
            builtObject.Setup();
            return builtObject;
        }
        [NonSerialized] public BuildingSO buildingSO;
        public Vector2Int pivotPos;
        public float rotation;
        public bool isBeingDestroyed;

        protected virtual void Setup()
        {
            Debug.LogWarning("Void setup in BuiltObject has not been overriden by a derived class", transform);
        }

        public override string ToString()
        {
            return "Built object at: " + pivotPos.x + ", " + pivotPos.y;
        }

        public virtual void DestroySelf()
        {
            foreach (Vector2Int gridPos in GetGridPosList())
            {
                GridBuildingSystem.Instance.grid.GetGridObject(gridPos).ClearBuiltObject();
            }
            SendBlockUpdate();
            Destroy(gameObject);
        }
        
        public IEnumerable<Vector2Int> GetGridPosList()
        {
            return buildingSO.GetGridPosList(pivotPos, rotation);
        }

        protected virtual void ReceiveBlockUpdate()
        {
            Debug.LogWarning("block update method called with no override", transform);
        }

        public void SendBlockUpdate()
        {
            //could have just used a case statement but this is way cooler
            List<BuiltObject> updatedObjects = new();
            int size1, size2, xLoopMultiplier, yLoopMultiplier;
            if (rotation % 180f == 0) {size1 = buildingSO.size.x; size2 = buildingSO.size.y;}
            else {size1 = buildingSO.size.y; size2 = buildingSO.size.x;}
            if (rotation % 270f == 0) xLoopMultiplier = 1;
            else {xLoopMultiplier = -1;}
            if (rotation < 180f) yLoopMultiplier = 1;
            else yLoopMultiplier = -1;
            for (int xLoop = 0; xLoop < size1; xLoop++)
            {
                UpdateObject(pivotPos.x + xLoop * xLoopMultiplier, pivotPos.y + 1 * -yLoopMultiplier);
                UpdateObject(pivotPos.x + xLoop * xLoopMultiplier, pivotPos.y + size2 * yLoopMultiplier);
            }
            for (int yLoop = 0; yLoop < size2; yLoop++)
            {
                UpdateObject(pivotPos.x + 1 * -xLoopMultiplier, pivotPos.y + yLoop * yLoopMultiplier);
                UpdateObject(pivotPos.x + size1 * xLoopMultiplier, pivotPos.y + yLoop * yLoopMultiplier);
            }

            void UpdateObject(int x, int y)
            {
                BuiltObject builtObject = GridBuildingSystem.Instance.grid.GetGridObject(x, y).GetBuiltObject();
                if (builtObject is null || updatedObjects.Contains(builtObject))
                {
                    Debug.Log($"failed to update belt at pos: ({x.ToString()},{y.ToString()})");
                    return;
                }
                Debug.Log($"updated belt at pos: ({x.ToString()},{y.ToString()})");
                updatedObjects.Add(builtObject);
                builtObject.ReceiveBlockUpdate();
            }
        }
    }
}