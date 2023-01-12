using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.BuildingGrid;
using ProjectAutomate.Items;
using UnityEngine;

namespace ProjectAutomate.Inventories
{
    public class InventoryBehaviour : BuiltObject, IInventory
    {
        [Serializable]
        protected class Dic : Dictionary<ItemSO, int> {}
        [SerializeField] protected Dic ItemsStored = new();
        [SerializeField] protected int itemCapacity;

        public int OutputPortCount { get; set; }

        public virtual ItemSO ExtractItems(int amount)
        {
            foreach (ItemSO itemType in ItemsStored.Keys.Where(itemType => ItemsStored[itemType] >= amount))
            {
                ItemsStored[itemType] -= amount;
                if (ItemsStored[itemType] > 0) return itemType;
                ItemsStored.Remove(itemType);
            }
            return null;
        }

        public virtual bool TryStoreItems(ItemSO itemType, int amount)
        {
            if (ItemsStored.Count == 0 && amount <= itemCapacity)
            {
                ItemsStored.Add(itemType, amount);
                return true;
            }
            if (!ItemsStored.ContainsKey(itemType) || ItemsStored[itemType] + amount > itemCapacity) return false;
            ItemsStored[itemType] += amount;
            return true;
        }
    }
}
