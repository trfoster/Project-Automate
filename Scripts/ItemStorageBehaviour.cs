using System.Linq;
using ProjectAutomate.Inventories;
using ProjectAutomate.Items;
using TMPro;
using UnityEngine;

namespace ProjectAutomate
{
    public sealed class ItemStorageBehaviour : InventoryBehaviour
    {
        private TextMeshPro countText;
        private SpriteRenderer itemSprite;

        public override ItemSO ExtractItems(int amount)
        {
            foreach (ItemSO itemType in ItemsStored.Keys.Where(itemType => ItemsStored[itemType] >= amount))
            {
                ItemsStored[itemType] -= amount;
                countText.text = ItemsStored[itemType].ToString(); //not in other inventories code
                if (ItemsStored[itemType] > 0) return itemType;
                ItemsStored.Remove(itemType);
                itemSprite.sprite = null; //not in other inventories code
                return itemType;
            }
            return null;
        }
        
        public override bool TryStoreItems(ItemSO itemType, int amount)
        {
            if (ItemsStored.Count == 0 && amount <= itemCapacity)
            {
                ItemsStored.Add(itemType, 0);
                itemSprite.sprite = itemType.sprite;
            }
            else if (!ItemsStored.ContainsKey(itemType) || ItemsStored[itemType] + amount > itemCapacity) return false;
            ItemsStored[itemType] += amount;
            countText.text = ItemsStored[itemType].ToString();
            return true;
        }
        

        protected override void Setup()
        {
            countText = GetComponentInChildren<TextMeshPro>();
            itemSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        /*private void OnEnable()
        {
            ContextDropdownBuilder builder = new ContextDropdownBuilder();
            builder.StartIndent("huaidhuwiahudiwhauidwnauidjhuwia");
            builder.AddItem("GameObject");
            builder.AddItem("Scriptable Object");
            builder.EndIndent();

            builder.GetResult().Show();
        }*/
    }
}
