using System.Collections.Generic;
using ProjectAutomate.Items;

namespace ProjectAutomate.Inventories
{
    public interface IInventory
    {
        ItemSO ExtractItems(int amount);
        bool TryStoreItems(ItemSO itemType, int amount);
        int OutputPortCount { get; set; }
        //int CurrentPort { get; set; }
    }

    /*public abstract class Inventory
    {
        public abstract bool StoreItems(ItemSO itemType, int amount);
    }*/
}