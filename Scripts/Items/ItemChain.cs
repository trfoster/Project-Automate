using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.Belts;
using UnityEngine;

namespace ProjectAutomate.Items
{
    [Serializable]
    public sealed class ItemChain
    {
        private List<IItem> itemList;
        [SerializeField] private List<Transform> itemTransformList;
        
        private IItem firstItem;
        [SerializeField] private Transform firstItemTransform;

        public bool firstItemIsStopped;
        private Transform nextItemDebugTransform;

        public void SetItemCanMove()
        {
            firstItemIsStopped = false;
        }

        public static void Create(IEnumerable<IItem> items)
        {
            IEnumerable<IItem> itemArray = items.ToArray();
            IItem firstItemTemp = itemArray.First();
            firstItemTemp.IsFirstItem = true;
            ItemChain itemChain = new ItemChain
            {
                firstItem = firstItemTemp, itemList = new List<IItem> {firstItemTemp},
                itemTransformList = new List<Transform> { ((ItemBehaviour)firstItemTemp).transform }
            };
            foreach (IItem item in itemArray.Skip(1)) itemChain.AddItemToList(item);
            itemChain.Setup();
        }

        private void Setup()
        {
            firstItemTransform = ((ItemBehaviour)firstItem).transform;
            //itemTransformList = new List<Transform> { firstItemTransform };
            //itemList = new List<IItem> { firstItem };
            TickSystem.OnAnimationTick += AnimationTickSystem_OnTick;
            Debug.Log("I am an item with a chain", ((ItemBehaviour)firstItem).transform);
            firstItem.ItemChain = this;
        }

        public void AddItemToList(IItem item)
        {
            itemList.Add(item);
            item.ItemChain = this;
            itemTransformList.Add(((ItemBehaviour)item).transform);
            Debug.Log("item list length: " + itemList.Count, ((ItemBehaviour)firstItem).transform);
        }

        public void RemoveFromItemList(IItem item, bool shouldDeleteItem)
        {
            int indexOfItem = itemList.IndexOf(item);
            Debug.Log($"Index of item being removed: {indexOfItem.ToString()}. ShouldRemoveItem: {shouldDeleteItem.ToString()}", (ItemBehaviour)item);
            itemList.Remove(item);
            itemTransformList.Remove(((ItemBehaviour)item).transform);
            if (shouldDeleteItem && itemList.Count == 0)
            {
                TickSystem.OnAnimationTick -= AnimationTickSystem_OnTick;
                return;
                //dont need to delete instance of item chain. with no references garbage collector will take care of it
            }
            
            if (indexOfItem == 0)
            {
                firstItem = itemList[0];
                if (!shouldDeleteItem)
                {
                    item.ItemChain = null;
                    Create(new[] { item });
                    /*if (firstItem.DistanceFromCenter > 0.25f)
                    {
                        itemList.Remove(firstItem);
                        firstItem.DestroySelf(firstItem.GetBeltUnderneath());
                        if (itemList.Count == 0)
                        {
                            TickSystem.OnAnimationTick -= AnimationTickSystem_OnTick;
                            return;
                            //dont need to delete instance of item chain. with no references garbage collector will take care of it
                        }

                        firstItem = itemList[0];
                    }*/
                }
                firstItem.IsFirstItem = true;
                firstItemTransform = ((ItemBehaviour)firstItem).transform;
            }
            else if (indexOfItem > 0 && indexOfItem < itemList.Count - 1)
            {
                List<IItem> itemListToBeRemoved = itemList.Skip(indexOfItem).ToList();
                //delete old part of chain from items
                foreach (IItem itemToBeRemoved in itemListToBeRemoved)
                {
                    itemToBeRemoved.ItemChain = null;
                }

                /*IItem firstItemOfRemoved = itemListToBeRemoved.First();
                if (!shouldRemoveItem && firstItemOfRemoved.DistanceFromCenter > 0.25f)
                {
                    itemListToBeRemoved.Remove(firstItemOfRemoved);
                    firstItemOfRemoved.DestroySelf(firstItemOfRemoved.GetBeltUnderneath());
                }*/
                //split chain
                Create(itemListToBeRemoved);
                itemList = itemList.Take(indexOfItem).ToList();
                Debug.Log($"remove item from item chain has been called with: {shouldDeleteItem.ToString()}");
                if (!shouldDeleteItem) // if item isnt actually removed from world
                {
                    Debug.Log("item being removed added", (ItemBehaviour)item);
                    itemList.Add(item);
                    itemTransformList = new List<Transform>(itemList.Select(i => ((ItemBehaviour)i).transform));
                    item.ItemChain = this;
                    return;
                }
                itemTransformList = new List<Transform>(itemList.Select(i => ((ItemBehaviour)i).transform));
            }
            else if (indexOfItem == itemList.Count - 1)
            {
                IItem lastItem = itemList[indexOfItem];
                lastItem.ItemChain = null;
                
                Create(new[] { lastItem });
                itemList.Remove(lastItem);
                if (!shouldDeleteItem) // if item isn't actually removed from world
                {
                    //TODO:Implement this
                }
                itemTransformList = new List<Transform>(itemList.Select(i => ((ItemBehaviour)i).transform));
            }
            //Debug.Log("item list length: " + itemList.Count, ((ItemBehaviour)firstItem).transform);
        }

        public int GetItemListCount()
        {
            return itemList?.Count ?? 0;
        }

        private void AnimationTickSystem_OnTick(object sender, TickSystem.OnTickEventArgs eventArgs)
        {
            if (firstItemIsStopped) return;
            bool firstItemHasMoved = firstItem.UpdateItemMovement();
            Debug.Log("if first item has moved: " + firstItemHasMoved, ((ItemBehaviour)firstItem).transform);
            if (!firstItemHasMoved)
            {
                firstItemIsStopped = true;
                return;
            }
            
            for (int x = 1; x < itemList.Count; x++)
            {
                itemList[x].UpdateItemMovement();
            }

            IItem nextItem = null;

            //Debug.LogWarning("Next belt: " + (BeltBehaviour)firstItem.GetNextBelt(),
                //(BeltBehaviour)firstItem.GetNextBelt());
            if (firstItem.DistanceFromCenter < 0f)
            {
                nextItem = firstItem.GetBeltUnderneath().GetItemList().FirstOrDefault(i => i.DistanceFromCenter >= 0f);
                nextItemDebugTransform = ((ItemBehaviour)nextItem)?.transform;
                if (nextItem is null) return;
                CheckForMerging(nextItem);
            }
            else
            {
                BeltBehaviour beltUnderneath = firstItem.GetBeltUnderneath();
                if (beltUnderneath.GetNextBelt() is { } nextBelt &&
                    nextBelt.GetInputDirection() == beltUnderneath.GetOutputDirection())
                {
                    nextItem = nextBelt.GetItemList()
                        .FirstOrDefault(i => i.DistanceFromCenter < 0f);
                }
                if (nextItem is null) return;
                CheckForMerging(nextItem);
            }
        }

        public void CheckForMerging(IItem nextItem)
        {
            if (nextItem.ItemChain == this ||
                !(Math.Abs(firstItem.DistanceFromCenter - nextItem.DistanceFromCenter) > 0.46875f) ||
                !(Math.Abs(firstItem.DistanceFromCenter - nextItem.DistanceFromCenter) < 0.53125f)) return;
            
            //Debug.Log("Item chains should merge !!", firstItemTransform);
            //Debug.Break();
            
            //merge item chains
            foreach (IItem item in itemList)
            {
                nextItem.ItemChain.AddItemToList(item);
            }

            itemList.Clear();
            TickSystem.OnAnimationTick -= AnimationTickSystem_OnTick;
            //dont need to delete instance of item chain. with no references garbage collector will take care of it
        }
    }
}