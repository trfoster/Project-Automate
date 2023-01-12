using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.BuildingGrid;
using ProjectAutomate.GameAssets;
using ProjectAutomate.Inventories;
using ProjectAutomate.Items;
using ProjectAutomate.Recipes;
using UnityEngine;

namespace ProjectAutomate.Machines
{
    public sealed class MachineBehaviour : BuiltObject, IInventory, IChangeState
    {
        //[Serializable] public class Dic : Dictionary<ItemSO, int> {}
        //public Dic inputItems;
        //public Dic outputItems;
        [Serializable] private class Dic : Dictionary<ItemSO, int>{}
        
        [SerializeField] private Dic inputItems;
        [SerializeField] private List<GameObject> inputItemSprites;
        [SerializeField] private Dic outputItems;
        [SerializeField] private List<GameObject> outputItemSprites;
        [SerializeField] private RecipeSO thisRecipe;
        [SerializeField] private int inputItemCapacity; //calculated as multiples of inputs
        [SerializeField] private int outputItemCapacity; //calculated as multiples of outputs
        [SerializeField] private bool isProcessing;
        [SerializeField] private int ticksProcessed;

        public int OutputPortCount { get; set; }

        public ItemSO ExtractItems(int amount)
        {
            foreach (ItemSO itemType in outputItems.Keys.Where(i => outputItems[i] >= amount))
            {
                outputItems[itemType] -= amount;
                return itemType;
            }
            return null;
        }

        public bool TryStoreItems(ItemSO itemType, int amount)
        {
            /*Debug.Log($"GetInputItemCapacity returns {GetInputItemCapacity(itemType)}");
            if (inputItems.Count < recipe.inputItems.Count && recipe.inputItems.ContainsKey(itemType) &&
                !inputItems.ContainsKey(itemType) && amount <= GetInputItemCapacity(itemType))
            {
                inputItems[itemType] += amount;
                return true;
            }*/
            if (!inputItems.ContainsKey(itemType) || inputItems[itemType] + amount > GetInputItemCapacity(itemType)) return false;
            inputItems[itemType] += amount;
            return true;
        }

        public void ChangeState()
        {
            SetRecipe(References.Instance.recipeReferences.GetNextRecipeSO(thisRecipe));
        }

        protected override void Setup()
        {
            TickSystem.OnAnimationTick += AnimationTickSystem_OnTick;
            inputItemSprites = new List<GameObject>();
            outputItemSprites = new List<GameObject>();
            SetRecipe(References.Instance.recipeReferences.ironToCopper);
        }

        private void SetRecipe(RecipeSO recipe)
        {
            thisRecipe = recipe;
            foreach (GameObject item in inputItemSprites.ToList())
            {
                inputItemSprites.Remove(item);
                Destroy(item);
            }
            foreach (GameObject item in outputItemSprites.ToList())
            {
                outputItemSprites.Remove(item);
                Destroy(item);
            }
            inputItems = new Dic();
            int index = 0;
            Transform thisTransform = transform;
            Vector2 thisPos = thisTransform.position;
            foreach (ItemSO itemType in thisRecipe.inputItems.Keys)
            {
                inputItems.Add(itemType, 0);
                for (int i = 0; i < thisRecipe.inputItems[itemType]; i++)
                {
                    //update visuals
                    int itemCount = thisRecipe.inputItems.Sum(key => key.Value) - 1;
                    float height =
                        (itemCount - index) * 0.5f - itemCount * 0.25f; //function for calculating item positions
                    //GameObject itemObject = Instantiate(newGameObject, new Vector3(thisPos.x - 1f, thisPos.y + height, -1f), Quaternion.identity, thisTransform);
                    GameObject itemObject = new GameObject($"Input Item Sprite {index}")
                    {
                        transform =
                        {
                            parent = thisTransform,
                            position = new Vector3(thisPos.x + 0.25f, thisPos.y + height + 1f, -3f)
                        }
                    };
                    itemObject.AddComponent<SpriteRenderer>().sprite = itemType.sprite;
                    inputItemSprites.Add(itemObject);
                    //itemObject.GetComponent<SpriteRenderer>().sprite = itemType.sprite;
                    index++;
                }
            }
            outputItems = new Dic();
            index = 0;
            foreach (ItemSO itemType in thisRecipe.outputItems.Keys)
            {
                outputItems.Add(itemType, 0);
                for (int i = 0; i < thisRecipe.outputItems[itemType]; i++)
                {
                    //update visuals
                    int itemCount = thisRecipe.outputItems.Sum(key => key.Value) - 1;
                    float height =
                        (itemCount - index) * 0.5f - itemCount * 0.25f; //function for calculating item positions
                    //GameObject itemObject = Instantiate(newGameObject, new Vector3(thisPos.x + 1f, thisPos.y + height, -1f), Quaternion.identity, thisTransform);
                    GameObject itemObject = new GameObject($"Output Item Sprite {index}")
                    {
                        transform =
                        {
                            parent = thisTransform,
                            position = new Vector3(thisPos.x + 1.75f, thisPos.y + height + 1f, -3f)
                        }
                    };
                    itemObject.AddComponent<SpriteRenderer>().sprite = itemType.sprite;
                    outputItemSprites.Add(itemObject);
                    //itemObject.GetComponent<SpriteRenderer>().sprite = itemType.sprite;
                    index++;
                }
            }
        }

        public override void DestroySelf()
        {
            TickSystem.OnAnimationTick -= AnimationTickSystem_OnTick;
            foreach (Vector2Int gridPos in GetGridPosList())
            {
                GridBuildingSystem.Instance.grid.GetGridObject(gridPos).ClearBuiltObject();
            }
            SendBlockUpdate();
            Destroy(gameObject);
        }

        private void AnimationTickSystem_OnTick(object sender, TickSystem.OnTickEventArgs eventArgs)
        {
            switch (isProcessing)
            {
                case false when inputItems.Count == 0 || thisRecipe.inputItems.Any(
                                    i => !inputItems.ContainsKey(i.Key) || inputItems[i.Key] < i.Value) ||
                                thisRecipe.outputItems.Any(i => outputItems[i.Key] + i.Value > GetOutputItemCapacity(i.Key)): 
                    return;
                case false:
                {
                    foreach ((ItemSO itemType, int amount) in thisRecipe.inputItems)
                    {
                        inputItems[itemType] -= amount;
                    }
                    break;
                }
            }
            isProcessing = true;
            ticksProcessed++;
            if (ticksProcessed < thisRecipe.ticksToProcess) return;
            foreach ((ItemSO itemType, int amount) in thisRecipe.outputItems)
            {
                outputItems[itemType] += amount;
            }
            isProcessing = false;
            ticksProcessed = 0;
        }

        private int GetInputItemCapacity(ItemSO itemType)
        {
            if (thisRecipe.inputItems.ContainsKey(itemType))
            {
                return thisRecipe.inputItems[itemType] * inputItemCapacity;
            }
            Debug.LogError("Machine behaviour GetInputItemCapacity has received an invalid item type");
            return 0;
        }

        private int GetOutputItemCapacity(ItemSO itemType)
        {
            if (thisRecipe.outputItems.ContainsKey(itemType))
            {
                return thisRecipe.outputItems[itemType] * outputItemCapacity;
            }
            Debug.LogError("Machine behaviour GetOutputItemCapacity has received an invalid item type");
            return 0;
        }
    }
}