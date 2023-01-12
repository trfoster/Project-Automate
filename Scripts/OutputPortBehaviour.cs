using System.Linq;
using ProjectAutomate.BuildingGrid;
using ProjectAutomate.GameAssets;
using ProjectAutomate.Inventories;
using ProjectAutomate.Items;
using ProjectAutomate.MyUtils;
using UnityEngine;

namespace ProjectAutomate
{
    public sealed class OutputPortBehaviour : Port
    {
        private Transform thisTransform;
        private Vector3 thisPos;
        private int ticksPerUpdate;
        private int ticksSinceLast = 32;
        private Vector3 spawnPos;
        private Transform itemPrefab;

        private void Awake()
        {
            TickSystem.BeforeAnimationTick += AnimationTickSystem_BeforeTick;
            itemPrefab = References.Instance.itemReferences.itemPrefab;
            thisTransform = transform;
            thisPos = thisTransform.position;
            //set spawn position
            Vector2 offset = MyMathf.GetBackwardVectorOffset(thisTransform.rotation.eulerAngles.z);
            offset *= 0.25f;
            spawnPos = new Vector3(thisPos.x + offset.x, thisPos.y + offset.y, -1f);
        }

        private void Start()
        {
            ticksPerUpdate = (int)(16 / Belt.GetSpeed() - 1);
            Debug.LogWarning("output port has null itemPrefab: " + (itemPrefab is null));
        }

        public void UpdateInventory()
        {
            //set spawn position
            Vector2 offset = MyMathf.GetBackwardVectorOffset(thisTransform.rotation.eulerAngles.z);
            Vector2 quarterOffset = offset * 0.25f;
            spawnPos = new Vector3(thisPos.x + quarterOffset.x, thisPos.y + quarterOffset.y, -1f);
            // set inventory
            Inventory = (IInventory)(GridBuildingSystem.Instance.grid.GetGridObject((Vector2)thisPos + offset)
                .GetBuiltObject());
        }

        private void AnimationTickSystem_BeforeTick(object sender, TickSystem.OnTickEventArgs eventArgs)
        {
            if (ticksSinceLast < ticksPerUpdate) { ticksSinceLast++; return;}
            int itemListLength = Belt.GetItemListCount();
            //Debug.Log("item list length: " + beltListLength, this);
            if (Belt.isBeingDestroyed) return;
            if (itemListLength > 1) return;
            IItem spawnedItem;
            ItemSO spawnedItemSo;
            switch (itemListLength)
            {
                case 0:
                    spawnedItemSo = Inventory.ExtractItems(1);
                    if (spawnedItemSo is null) return;
                    spawnedItem = ItemBehaviour.Create(spawnedItemSo, Belt, itemPrefab, spawnPos);
                    Belt.AddToItemList(spawnedItem);
                    ItemChain.Create(new[]{spawnedItem} );
                    break;
                case 1:
                    IItem existingItem = Belt.GetItemList().Last();
                    if (existingItem.DistanceFromCenter < 0.25f) return;
                    spawnedItemSo = Inventory.ExtractItems(1);
                    if (spawnedItemSo is null) return;
                    bool shouldAddToItemChain = existingItem.DistanceFromCenter == 0.25f;
                    //Debug.Log("existing item: " + existingItem, ((ItemBehaviour)existingItem).transform);
                    //Debug.Log("should add to chain: " + shouldAddToItemChain);
                    //Debug.Break();
                    //Debug.Log("item isn't null");
                    spawnedItem = ItemBehaviour.Create(spawnedItemSo, Belt, itemPrefab, spawnPos);
                    Belt.AddToItemList(spawnedItem);
                    if (shouldAddToItemChain) existingItem.ItemChain.AddItemToList(spawnedItem);
                    else ItemChain.Create(new[] { spawnedItem });
                    break;
            }
            ticksSinceLast = 0; //has to be here
        }

        private void OnDestroy()
        {
            TickSystem.BeforeAnimationTick -= AnimationTickSystem_BeforeTick;
        }
    }
}