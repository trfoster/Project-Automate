using System;
using System.Linq;
using ProjectAutomate.Belts;
using ProjectAutomate.BuildingGrid;
using ProjectAutomate.MyUtils;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectAutomate.Items
{
    public sealed class ItemBehaviour : MonoBehaviour, IItem
    {
        private float moveDistance;
        public bool debug;
        private float moveDirection;
        private IItem thisItem;
        private BeltBehaviour beltUnderneath;
        private BeltBehaviour prevBelt;
        public float DistanceFromCenter { get; private set; }

        public ItemSO ItemSo { get; private set; }

        public void DestroySelf(BeltBehaviour belt)
        {
            //int index = 0;
            /*if (belt.GetItemListCount() < 3)
            {
                index = belt.GetItemList().ToList().IndexOf(thisItem);
            }*/
            int index = belt.GetItemList().ToList().IndexOf(thisItem);
            if (index != -1)
            {
                Debug.Log($"index that item is going to be removed at: {index.ToString()}");
                if (Input.GetKey(KeyCode.G)) Debug.Break();
                belt.RemoveFromItemList(index);
            }
            _itemChain?.RemoveFromItemList(thisItem, true);
            Destroy(gameObject);
        }

        [SerializeField] private ItemChain _itemChain;

        public ItemChain ItemChain
        {
            get => _itemChain;
            set => _itemChain = value;
        }
        
        public bool IsFirstItem { get; set; }

        public static IItem Create(ItemSO itemType, BeltBehaviour beltUnderneath, Transform itemPrefab, Vector3 pos)
        {
            Debug.Log("item create method has null itemPrefab: " + (itemPrefab == null));
            Transform itemTransform = Instantiate(itemPrefab, pos, quaternion.identity);
            ItemBehaviour itemBehaviour = itemTransform.GetComponent<ItemBehaviour>();
            itemBehaviour.beltUnderneath = beltUnderneath;
            itemBehaviour.ItemSo = itemType;
            itemTransform.GetComponent<SpriteRenderer>().sprite = itemType.sprite;
            itemBehaviour.ItemSo.nameString = itemBehaviour.ItemSo.material + " " + itemBehaviour.ItemSo.type;
            itemBehaviour.DistanceFromCenter = -0.25f; //-0.75f
            return itemBehaviour.thisItem;
        }

        private void Awake()
        {
            thisItem = GetComponent<IItem>();
        }

        public bool UpdateItemMovement()
        {
            Transform thisTransform = transform;
            Vector3 thisPos = thisTransform.position;

            //Vector3 buildingPos = MyMathf.RoundDirectional(thisPos, moveDirection);
            //BuiltObject builtObject = GridBuildingSystem.Instance.grid.GetGridObject(buildingPos).GetBuiltObject();
            //if (builtObject is null || builtObject.isBeingDestroyed) return false;
            //BeltBehaviour beltUnderneath = (BeltBehaviour)builtObject;
            if (DistanceFromCenter > 0.5f)
            {
                /*Vector3 prevPos = new Vector3(thisPos.x + 0.3f * -math.sin(moveDirection),
                    thisPos.y + 0.3f * math.cos(moveDirection), 0f);
                Debug.Log("prevPos: " + prevPos.ToString("F10"), thisTransform);
                if (GridBuildingSystem.Instance.grid.GetGridObject(prevPos).GetBuiltObject() is BeltBehaviour prevBelt)
                {
                    prevBelt.RemoveFromItemList(thisItem);
                }*/
                /*prevBelt = beltUnderneath;
                prevBeltTransform = ((BeltBehaviour)prevBelt).transform;
                beltUnderneath = (BeltBehaviour)GridBuildingSystem.Instance.grid.GetGridObject(MyMathf.RoundDirectional(thisPos, moveDirection)).GetBuiltObject();
                beltUnderneathTransform = ((BeltBehaviour)beltUnderneath).transform;
                prevBelt?.RemoveFromItemList(thisItem);
                beltUnderneath.AddToItemList(thisItem);
                DistanceFromCenter = -1f + DistanceFromCenter;*/
                
                prevBelt = beltUnderneath;
                //beltUnderneath = (BeltBehaviour)GridBuildingSystem.Instance.grid.GetGridObject(MyMathf.RoundDirectional(thisPos, moveDirection)).GetBuiltObject();
                beltUnderneath = prevBelt.GetNextBelt();
                int index = 0;
                if (prevBelt.GetItemListCount() < 3)
                {
                    index = prevBelt.GetItemList().ToList().IndexOf(thisItem);
                }
                prevBelt.RemoveFromItemList(index);
                if (beltUnderneath is null) Debug.Break();
                beltUnderneath.AddToItemList(thisItem);
                DistanceFromCenter = -1f + DistanceFromCenter;
                
            }
            else
            {
                moveDirection = DistanceFromCenter switch
                {
                    0f => beltUnderneath.GetOutputDirection(),
                    // -0.75f                                                                                                                                                                                           
                    -0.25f => beltUnderneath.GetInputDirection(),
                    _ => moveDirection
                };
            }
            //checks to see if there is a belt after this one
            if (IsFirstItem && Math.Abs(DistanceFromCenter - 0.25f) < 0.0001f)
            {
                //TODO: this could be done with the getvectoroffset so no need for switch
                switch (moveDirection)
                {
                    case 0f:                                                                                                                                                                                            
                        if (GridBuildingSystem.Instance.grid.GetGridObject(thisPos.x, thisPos.y - 0.75f).GetBuiltObject() is not BeltBehaviour bottomBelt ||                                                                    
                            bottomBelt.GetInputDirection() != beltUnderneath.GetOutputDirection()) return false;
                        break;                                                                                                                                                                                          
                    case 90f:                                                                                                                                                                                           
                        if (GridBuildingSystem.Instance.grid.GetGridObject(thisPos.x + 0.75f, thisPos.y).GetBuiltObject() is not BeltBehaviour rightBelt ||                                                                     
                            rightBelt.GetInputDirection() != beltUnderneath.GetOutputDirection()) return false;
                        break;                                                                                                                                                                                          
                    case 180f:                                                                                                                                                                                          
                        if (GridBuildingSystem.Instance.grid.GetGridObject(thisPos.x, thisPos.y + 0.75f).GetBuiltObject() is not BeltBehaviour topBelt ||                                                                       
                            topBelt.GetInputDirection() != beltUnderneath.GetOutputDirection()) return false;
                        break;                                                                                                                                                                                          
                    case 270f:                                                                                                                                                                                          
                        if (GridBuildingSystem.Instance.grid.GetGridObject(thisPos.x - 0.75f, thisPos.y).GetBuiltObject() is not BeltBehaviour leftBelt ||                                                                      
                            leftBelt.GetInputDirection() != beltUnderneath.GetOutputDirection()) return false;
                        break;
                }

                /*Vector2 thisPosition = new Vector2(thisPos.x, thisPos.y);
                if (GridBuildingSystem.Instance.grid.GetGridObject(beltUnderneath.GetPos() + MyMathf.GetForwardVectorOffset(moveDirection)).GetBuiltObject() is not BeltBehaviour nextBelt ||
                nextBelt.GetInputDirection() != beltUnderneath.GetOutputDirection()) return false;*/
            }
            if (debug) Debug.Log("move direction: " + moveDirection, this);
            moveDistance = beltUnderneath.GetSpeed() / 32f;
            switch (moveDirection)
            {
                case 0f:
                    thisTransform.position = new Vector3(thisPos.x, thisPos.y - moveDistance, thisPos.z);
                    DistanceFromCenter += moveDistance;
                    break;
                case 90f:
                    thisTransform.position = new Vector3(thisPos.x + moveDistance, thisPos.y, thisPos.z);
                    DistanceFromCenter += moveDistance;
                    break;
                case 180f:
                    thisTransform.position = new Vector3(thisPos.x, thisPos.y + moveDistance, thisPos.z);
                    DistanceFromCenter += moveDistance;
                    break;
                case 270f:
                    thisTransform.position = new Vector3(thisPos.x - moveDistance, thisPos.y, thisPos.z);
                    DistanceFromCenter += moveDistance;
                    break;
            }
            if (debug) Debug.Log("distance from center: " + DistanceFromCenter.ToString("F5"), this);
            return true;
        }

        public void CorrectBackItemPos(float inputDirection, Vector2 beltPos, bool shouldRemoveFromList)
        {
            //Transform thisTransform = transform;
            /*thisTransform.position = inputDirection switch
            {
                0f => new Vector3(beltPos.x, beltPos.y - DistanceFromCenter, -1f),
                90f => new Vector3(beltPos.x + DistanceFromCenter, beltPos.y, -1f),
                180f => new Vector3(beltPos.x, beltPos.y + DistanceFromCenter, -1f),
                270f => new Vector3(beltPos.x - DistanceFromCenter, beltPos.y, -1f),
                _ => thisTransform.position
            };*/
            Vector2 pos = beltPos + MyMathf.GetForwardVectorOffset(inputDirection) * DistanceFromCenter;
            transform.position =  new Vector3(pos.x, pos.y, -1f);
            moveDirection = inputDirection;

            //TODO: make it so it has to delete the item which will force ItemChain.RemoveFromItemList to split chain properly
            //remove from old chain
            //if (!shouldRemoveFromList) return;
            //_itemChain.RemoveFromItemList(thisItem, false);
        }

        /*public void CheckForItemInFront()
        {
            Transform thisTransform = transform;
            Vector3 thisPos = thisTransform.position;

            //whether it needs to check this belt or the next
            if (DistanceFromCenter <= 0f && )
            {
                
            }
        }*/

        public BeltBehaviour GetBeltUnderneath()
        {
            return beltUnderneath;
        }
    }
}