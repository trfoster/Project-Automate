using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.BuildingSOs;
using ProjectAutomate.Items;
using ProjectAutomate.Recipes;
using UnityEngine;

namespace ProjectAutomate.GameAssets
{
    [Serializable]
    public sealed class References : MonoBehaviour
    {
        //public static References Instance => _Instance ??= Instantiate(Resources.Load<References>("References"));

        //public static References Instance => _Instance ? _Instance : _Instance = Instantiate(Resources.Load<References>("References"));
        /*public static References Instance
        {
            get
            {
                if (_Instance is null) _Instance = Instantiate(Resources.Load<References>("References"));
                return _Instance;
            }
        }
        If this one is used then the _Instance = this; line in Awake() is not needed.
        */

        public static void InstantiateReferences()
        {
            Instance = Instantiate(Resources.Load<References>("References"));
            Instance.itemReferences.Setup();
            Instance.recipeReferences.Setup();
            Instance.buildingReferences.Setup();
        }

        public static References Instance { get; private set; }

        public RuntimeAnimatorController animSyncController;
        public Transform inputPortPrefab;
        public Transform outputPortPrefab;
        
        [Serializable]
        public class ItemReferences
        {
            internal List<ItemSO> ItemList;
            internal void Setup()
            {
                ItemList = new List<ItemSO>
                {
                    copperIngot,
                    goldIngot,
                    ironIngot,
                    tungstenIngot
                };
            }

            public ItemSO copperIngot;
            public ItemSO goldIngot;
            public ItemSO ironIngot;
            public ItemSO tungstenIngot;
            
            public ItemSO GetNextItemSO(ItemSO item)
            {
                return item == ItemList.Last() ? ItemList.First() : ItemList[ItemList.IndexOf(item) + 1];
            }

            public Transform itemPrefab;
        }
        public ItemReferences itemReferences;

        
        [Serializable]
        public class BuildingReferences
        {
            internal List<BuildingSO> BuildingList;
            internal void Setup()
            {
                BuildingList = new List<BuildingSO>
                {
                    belt,
                    machine,
                    itemSource,
                    itemStorage,
                    junction
                };
            }
            
            public BuildingSO belt;
            public BuildingSO machine;
            public BuildingSO itemSource;
            public BuildingSO itemStorage;
            public BuildingSO junction;

            public BuildingSO GetNextBuilding(BuildingSO building)
            {
                return building == BuildingList.Last() ? BuildingList.First() : BuildingList[BuildingList.IndexOf(building) + 1];
            }
        }
        public BuildingReferences buildingReferences;

        [Serializable]
        public class RecipeReferences
        {
            internal List<RecipeSO> RecipeList;
            internal void Setup()
            {
                RecipeList = new List<RecipeSO>
                {
                    copperToGold,
                    goldToTungsten,
                    ironToCopper,
                    ironToIron
                };
            }

            public RecipeSO copperToGold;
            public RecipeSO goldToTungsten;
            public RecipeSO ironToCopper;
            public RecipeSO ironToIron;

            public RecipeSO GetNextRecipeSO(RecipeSO recipe)
            {
                return recipe == RecipeList.Last() ? RecipeList.First() : RecipeList[RecipeList.IndexOf(recipe) + 1];
            }
        }
        public RecipeReferences recipeReferences;
    }
}
