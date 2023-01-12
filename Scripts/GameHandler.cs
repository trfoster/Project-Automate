using System;
using ProjectAutomate.GameAssets;
using UnityEngine;

namespace ProjectAutomate
{
    public sealed class GameHandler : MonoBehaviour
    {
        //public bool showAdvancedDebug = false;
        //public List<Vector2Int> debugPositions;
        
        private static GameObject AnimSyncGameObject;
        public static Animator AnimSyncAnimator;

        private void Start()
        {
            References.InstantiateReferences();
            TickSystem.Create();
            AnimSyncGameObject = new GameObject("Animation Syncer");
            AnimSyncAnimator = AnimSyncGameObject.AddComponent<Animator>();
            AnimSyncAnimator.runtimeAnimatorController = References.Instance.animSyncController;
            //grid.OnGridValueChanged += Grid_OnGridValueChanged;
            //Debug.Log("length of list: " + debugPositions.Count);
        }

        /*private void Grid_OnGridValueChanged(object sender, System.EventArgs e)
    {
        foreach(Vector2Int gridPos in debugPositions)
		{
            Debug.Log(gridPos, grid.GetInfo(gridPos).gameObject);
            GridInfo gridPosInfo = grid.GetInfo(gridPos);
            Debug.Log("isRotatable: " + gridPosInfo.isRotatable);
            Debug.Log("isBelt: " + gridPosInfo.isBelt);
            if (gridPosInfo.isBelt && showAdvancedDebug)
            {
                BeltBehaviour gridPosBelt = gridPosInfo.belt;
                Debug.Log("     output direction: " + gridPosBelt.outputDirection);
                Debug.Log("     input direction: " + gridPosBelt.inputDirection);
                Debug.Log("     speed: " + gridPosBelt.speed);
            }
            Debug.Log("is fluid container: " + gridPosInfo.isFluidContainer);
            if (gridPosInfo.isFluidContainer && showAdvancedDebug)
            {
                FluidContainer gridPosFluidContainer = gridPosInfo.fluidContainer;
                Debug.Log("     max content: " + gridPosFluidContainer.maxContent);
                Debug.Log("     content: " + gridPosFluidContainer.content);
                Debug.Log("     previous content: " + gridPosFluidContainer.prevContent);
                Debug.Log("     connection count: " + gridPosFluidContainer.connectionCount);
            }
            Debug.Log("hasInventory: " + gridPosInfo.hasInventory);
            if (gridPosInfo.hasInventory && showAdvancedDebug)
            {
                Inventory gridPosInventory = gridPosInfo.inventory;
                Debug.Log("     itemSo capacity: " + gridPosInventory.itemCapacity);
                Debug.Log("     itemSo number: " + gridPosInventory.items.Length);
                Debug.Log("     items: " + gridPosInventory.items);
            }
            // machine as well. didn't add it now because there isn't a need for it yet
        }
        if (debugPositions.Count != 0) Debug.Log("Has finished debugging " + debugPositions.Count + " positions");
    }*/
    }
}
