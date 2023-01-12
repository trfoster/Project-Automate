using ProjectAutomate.BuildingGrid;
using ProjectAutomate.GameAssets;
using ProjectAutomate.Inventories;
using ProjectAutomate.Items;
using UnityEngine;

namespace ProjectAutomate
{
    public sealed class ItemSourceBehaviour : BuiltObject, IInventory, IChangeState
    {
        private ItemSO outputType;
        private SpriteRenderer itemSprite;
        
        public ItemSO ExtractItems(int amount)
        {
            return outputType;
        }

        public bool TryStoreItems(ItemSO itemType, int amount)
        {
            return true;
        }
        
        public int OutputPortCount { get; set; }

        protected override void Setup()
        {
            outputType = References.Instance.itemReferences.copperIngot;
            //Debug.Log("output: " + outputType);
            itemSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
            itemSprite.sprite = outputType.sprite;
        }

        public void ChangeState()
        {
            outputType = References.Instance.itemReferences.GetNextItemSO(outputType);
            itemSprite.sprite = outputType.sprite;
        }
    }
}
