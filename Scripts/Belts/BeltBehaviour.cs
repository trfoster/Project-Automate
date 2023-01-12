using System;
using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.BuildingGrid;
using ProjectAutomate.GameAssets;
using ProjectAutomate.Inventories;
using ProjectAutomate.Items;
using ProjectAutomate.MyUtils;
using Sirenix.Serialization;
using UnityEngine;

namespace ProjectAutomate.Belts
{
    public sealed class BeltBehaviour : BuiltObject, IChangeState
    {
        private bool inputPortMade;
        private bool outputPortMade;
        [SerializeField] private bool debug;
        private bool isInputToBuilding;
        private bool isOutputFromBuilding;
        //private bool shouldRemainCorner = true;
        /*TODO
          things done:
          improved efficiency of get belt underneath for items, removed IBelt,
          fix null beltunderneath bug when making sraight and adding corner,
          check BeltBehaviour is working
          fix possible inputs with tuples, make then choosable with rules: 
            rot = rot with same level
            >rot = rot with higher level
            - if rot is added, dont switch to it.
            - if >rot is added, switch to it.
             
          fix bug: when switching input direction, remove port
          check efficiency of grid arrays(Removed generics)
          Use new method where possible(only in this script)
          
          Needs doing:
          PORTS
          Splitting chains without deleting items
          */
        
        [SerializeField] private RuntimeAnimatorController cornerController;
        private RuntimeAnimatorController straightController;
        private AnimationSync thisAnimationSync;
        private Animator thisAnimator;

        private BuiltObject topObject;
        private BuiltObject rightObject;
        private BuiltObject leftObject;
        private BuiltObject bottomObject;

        private BeltBehaviour nextBelt;
        private BeltBehaviour prevBelt;
        private BeltBehaviour lastPrevBelt; // The previous prevBelt before switching to the new one

        [SerializeField] private float speed = 1f;

        //private float preferredInputDirection;
        [OdinSerialize]
        private (float direction, bool isBelt) currentInput; //the selected input from the list of possible ones
        private int rotationIndex;
        private float[] inputDirections;
        [OdinSerialize]
        private (float direction, bool isBelt)[] inputs;
        //private List<float> inputDirections;
        //private bool isAdjacentInventory;

        private Transform inputPortPrefab;
        private Transform outputPortPrefab;

        // TODO: Maybe set the speed in a SO and set it here
        public float GetSpeed()
        {
            return speed;
        }

        private float outputDirection;
        public float GetOutputDirection()
        {
            return outputDirection;
        }
    
        private float inputDirection;
        public float GetInputDirection()
        {
            return inputDirection;
        }

        public BeltBehaviour GetNextBelt()
        {
            return nextBelt;
        }

        /*private void SetPrevBelt(BeltBehaviour previousBelt)
        {
            prevBelt = previousBelt;
        }*/

        private bool isCorner;
        public bool IsCorner()
        {
            return isCorner;
        }

        private List<IItem> itemList;
        [SerializeField] private List<Transform> itemTransformList;
        public IEnumerable<IItem> GetItemList()
        {
            return itemList;
        }

        public int GetItemListCount()
        {
            return itemList?.Count ?? 0;
        }

        public void AddToItemList(IItem item)
        {
            itemList.Add(item);
            itemTransformList.Add(((ItemBehaviour)item).transform);
        }

        public void RemoveFromItemList(int index)
        {
            itemList.RemoveAt(index);
            itemTransformList.RemoveAt(index);
            if (Input.GetKey(KeyCode.G)) Debug.Break();
        }
        
        public void ChangeState() //cycles the preferred input direction
        {
            UpdateInputDirections();
            Debug.Log("no. of possible inputs: " + inputDirections.Length);
            foreach (var rot in inputDirections) Debug.Log("possible rotation: " + rot);
            Debug.Log("preferred input direction before: " + currentInput.direction);
            if (inputDirections.Length < 2) return;
            rotationIndex++;
            if (rotationIndex > inputDirections.Length - 1) rotationIndex = 0;
            currentInput.direction = inputDirections[rotationIndex];
            Debug.Log("preferred input direction after: " + currentInput.direction);
            ReceiveBlockUpdate();
        }
        
        private void UpdateInputDirections()
        {
            List<(float direction, bool isBelt)> inputList = new();
            if (rotation != 180f)
            {
                switch (topObject)
                {
                    case IInventory: inputList.Add((0f, false)); 
                        break;
                    case BeltBehaviour topBelt when topBelt.GetOutputDirection() == 0f: inputList.Add((0f, true));
                        break;
                }
            }
            if (rotation != 90f)
            {
                switch (rightObject)
                {
                    case IInventory: inputList.Add((270f, false));
                        break;
                    case BeltBehaviour rightBelt when rightBelt.GetOutputDirection() == 270f: inputList.Add((270f, true));
                        break;
                }
            }
            if (rotation != 0f)
            {
                switch (bottomObject)
                {
                    case IInventory: inputList.Add((180f, false));
                        break;
                    case BeltBehaviour bottomBelt when bottomBelt.GetOutputDirection() == 180f: inputList.Add((180f, true));
                        break;
                }
            }
            if (rotation != 270f)
            {
                switch (leftObject)
                {
                    case IInventory: inputList.Add((90f, false));
                        break;
                    case BeltBehaviour leftBelt when leftBelt.GetOutputDirection() == 90f: inputList.Add((90f, true));
                        break;
                }
            }
            if (inputList.Count < 1)
            {
                inputDirections = new float[]{};
                inputs = new (float direction, bool isBelt)[]{};
                currentInput.direction = -1f;
                currentInput.isBelt = false;
                return;
            }
            inputList = inputList.OrderByDescending(p => p.isBelt).ToList();
            inputs = inputList.ToArray();
            inputDirections = inputList.Select(p => p.direction).ToArray();
            if (rotationIndex > inputDirections.Length - 1) rotationIndex = inputDirections.Length - 1;
            foreach (var rot in inputDirections) Debug.Log("possible input: " + rot);
            Debug.Log("index: " + rotationIndex);
            //this makes sure that the current preferred input is kept
            if (inputDirections.Contains(currentInput.direction))
            {
                if (!currentInput.isBelt && inputs[0].isBelt) rotationIndex = 0;
                else rotationIndex = Array.IndexOf(inputDirections, currentInput.direction);
                currentInput = inputs[rotationIndex];
                return;
            }
            //if the last preferred input was removed. ony gets run from receiveUpdate
            //currentInput.direction = inputDirections[rotationIndex];
            currentInput = inputs[0];
        }

        protected override void Setup()
        {
            straightController = GetComponent<Animator>().runtimeAnimatorController;
            thisAnimationSync = GetComponent<AnimationSync>();
            thisAnimator = GetComponent<Animator>();
            inputPortPrefab = References.Instance.inputPortPrefab;
            outputPortPrefab = References.Instance.outputPortPrefab;
            SetDirections();
            itemList = new List<IItem>();
            itemTransformList = new List<Transform>();
            currentInput.direction = -1f;
            prevBelt = null;
            lastPrevBelt = null;
            ReceiveBlockUpdate();
        }

        public override void DestroySelf()
        {
            isBeingDestroyed = true;
            foreach (IItem item in itemList.ToList().AsEnumerable())
            {
                //Debug.Log("I tried to kill an item");
                item.DestroySelf(this);
            }
            if (GridBuildingSystem.Instance.grid.GetGridObject(pivotPos + MyMathf.GetBackwardVectorOffset(inputDirection))
                    .GetBuiltObject() is BeltBehaviour belt)
            {
                foreach (IItem item in belt.GetItemList().ToArray().Where(item => item.DistanceFromCenter > 0.25f)
                             .AsEnumerable().Reverse())
                {
                    //Debug.Log("I tried to kill an item");
                    item.DestroySelf(belt);
                }
            }
            /*foreach (Vector2Int gridPos in GetGridPosList())
            {
                GridBuildingSystem.Instance.grid.GetGridObject(gridPos).ClearBuiltObject();
            }*/
            GridBuildingSystem.Instance.grid.GetGridObject(pivotPos).ClearBuiltObject();
            SendBlockUpdate();
            Destroy(gameObject);
        }

        protected override void ReceiveBlockUpdate()
        {
            //Debug.Log(name + " has received an update", this);
            isInputToBuilding = false;
            isOutputFromBuilding = false;
            lastPrevBelt = prevBelt;

            topObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x, pivotPos.y + 1).GetBuiltObject();
            rightObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x + 1, pivotPos.y).GetBuiltObject();
            leftObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x - 1, pivotPos.y).GetBuiltObject();
            bottomObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x, pivotPos.y - 1).GetBuiltObject();
            UpdateInputDirections();
            //bool isNotBeltBehind = IsBeltBehind();

            prevBelt = null;
            BuiltObject builtObject = GetObjectAtRotation(currentInput.direction, true);
            if (builtObject is not null)
            {
                if (currentInput.isBelt) prevBelt = (BeltBehaviour)builtObject;
                else
                {
                    isOutputFromBuilding = true;
                    if (!outputPortMade) MakeOutputPort((IInventory)builtObject);
                }
            }

            /*switch (currentInput.direction)
            {
                case 0f:
                    if (currentInput.isBelt) { prevBelt = (BeltBehaviour)topObject; break; }
                    isOutputFromBuilding = true;
                    if (!outputPortMade) MakeOutputPort((IInventory)topObject);
                    break;
                case 90f:
                    if (currentInput.isBelt) { prevBelt = (BeltBehaviour)leftObject; break; }
                    isOutputFromBuilding = true;
                    if (!outputPortMade) MakeOutputPort((IInventory)leftObject);
                    break;
                case 180f:
                    if (currentInput.isBelt) { prevBelt = (BeltBehaviour)bottomObject; break; }
                    isOutputFromBuilding = true;
                    if (!outputPortMade) MakeOutputPort((IInventory)bottomObject);
                    break;
                case 270f:
                    if (currentInput.isBelt) { prevBelt = (BeltBehaviour)rightObject; break; }
                    isOutputFromBuilding = true;
                    if (!outputPortMade) MakeOutputPort((IInventory)rightObject);
                    break;
            }*/
            
            
            switch (outputDirection)
            {
                case 0f:
                    switch (bottomObject)
                    {
                        case IInventory bottomInv:
                            if (!inputPortMade) MakeInputPort(bottomInv);
                            isInputToBuilding = true;
                            break;
                        case BeltBehaviour bottomBelt3 when bottomBelt3.GetInputDirection() == 0f:
                            nextBelt = bottomBelt3;
                            break;
                        default:
                            nextBelt = null;
                            break;
                    }
                    switch (currentInput.direction)
                    {
                        case 0f or -1f: MakeStraight();
                            break;
                        case 270f: MakeCorner(new Vector3(0f, 180f, 90f));
                            break;
                        case 90f: MakeCorner(new Vector3(0f, 0f, 90f));
                            break;
                    }
                    break;
                case 90f:
                    switch (rightObject)
                    {
                        case IInventory rightInv:
                            if (!inputPortMade) MakeInputPort(rightInv);
                            isInputToBuilding = true;
                            break;
                        case BeltBehaviour rightBelt3 when rightBelt3.GetInputDirection() == 90f:
                            nextBelt = rightBelt3;
                            break;
                        default:
                            nextBelt = null;
                            break;
                    }
                    switch (currentInput.direction)
                    {
                        case 90f or -1f: MakeStraight();
                            break;
                        case 0f: MakeCorner(new Vector3(0f, 180f, 0f));
                            break;
                        case 180f: MakeCorner(new Vector3(0f, 0f, 180f));
                            break;
                    }
                    break;
                case 180f:
                    switch (topObject)
                    {
                        case IInventory topInv:
                            if (!inputPortMade) MakeInputPort(topInv);
                            isInputToBuilding = true;
                            break;
                        case BeltBehaviour topBelt3 when topBelt3.GetInputDirection() == 180f:
                            nextBelt = topBelt3;
                            break;
                        default:
                            nextBelt = null;
                            break;
                    }
                    switch (currentInput.direction)
                    {
                        case 180f or -1f: MakeStraight();
                            break;
                        case 270f: MakeCorner(new Vector3(0f, 0f, 270f));
                            break;
                        case 90f: MakeCorner(new Vector3(0f, 180f, 270f));
                            break;
                    }
                    break;
                case 270f:
                    switch (leftObject)
                    {
                        case IInventory leftInv:
                            if (!inputPortMade) MakeInputPort(leftInv);
                            isInputToBuilding = true;
                            break;
                        case BeltBehaviour leftBelt3 when leftBelt3.GetInputDirection() == 270f:
                            nextBelt = leftBelt3;
                            break;
                        default:
                            nextBelt = null;
                            break;
                    }
                    switch (currentInput.direction)
                    {
                        case 270f or -1f: MakeStraight();
                            break;
                        case 0f: MakeCorner(new Vector3(0f, 0f, 0f));
                            break;
                        case 180f: MakeCorner(new Vector3(180f, 0f, 0f));
                            break;
                    }
                    break;
                default:
                    Debug.Log("output direction has not identified with any angle", transform);
                    break;
            }

            if (prevBelt is not null && prevBelt.GetItemListCount() > 0)
            {
                prevBelt.GetItemList().First().ItemChain.SetItemCanMove();
            }

            //Debug.Break();
            //if (debug) Debug.Log("Has reached port detection", this);
            foreach (Port port in GetComponentsInChildren<Port>())
            {
                Transform portTransform = port.transform;
                //Debug.Log($"is output from building: {isOutputFromBuilding.ToString()}", this);
                switch (port)
                {
                    //if (debug) Debug.Log(port.name, this);
                    case InputPortBehaviour when isInputToBuilding:
                    {
                        //Vector3 portPos = portTransform.position;
                        //portTransform.position = new Vector3(portPos.x, portPos.y, -2f);
                        portTransform.rotation = Quaternion.Euler(0f, 0f, outputDirection);
                        break;
                    }
                    case InputPortBehaviour:
                    {
                        Destroy(port.gameObject);
                        inputPortMade = false;
                        break;
                    }
                    case OutputPortBehaviour outputPort when isOutputFromBuilding:
                    {
                        //Vector3 portPos = portTransform.position;
                        //portTransform.position = new Vector3(portPos.x, portPos.y, -2f);
                        portTransform.rotation = Quaternion.Euler(0f, 0f, inputDirection);
                        //needed because inventory can change when belt switches input
                        outputPort.UpdateInventory();
                        break;
                    }
                    case OutputPortBehaviour:
                    {
                        Destroy(port.gameObject);
                        outputPortMade = false;
                        break;
                    }
                }

                /*if (port is InputPortBehaviour)
                {
                    if (isInputToBuilding)
                    {
                        Vector3 portPos = portTransform.position;
                        portTransform.position = new Vector3(portPos.x, portPos.y, -2f);
                        portTransform.rotation = Quaternion.Euler(0f, 0f, outputDirection);
                    }
                    else
                    {
                        Destroy(port.gameObject);
                        inputPortMade = false;
                    }
                }
                else if (port is OutputPortBehaviour)
                {
                    if (isOutputFromBuilding)
                    {
                        Vector3 portPos = portTransform.position;
                        portTransform.position = new Vector3(portPos.x, portPos.y, -2f);
                        portTransform.rotation = Quaternion.Euler(0f, 0f, inputDirection);
                    }
                    else
                    {
                        Destroy(port.gameObject);
                        outputPortMade = false;
                    }
                }*/
                
            }
            //let the item know that it probably will be able to move again
            /*Debug.Log("I am a belt", transform);
            if (itemList.FirstOrDefault() is IItem firstItem &&
                GridBuildingSystem.Instance.grid
                    .GetGridObject(pivotPos + MyMathf.GetForwardVectorOffset(outputDirection))
                    .GetBuiltObject() is BeltBehaviour beltInFront && beltInFront.GetInputDirection() == outputDirection &&
                (beltInFront.GetItemListCount() == 0 || !beltInFront.GetItemList().Any(item => item.DistanceFromCenter == -0.25f)))
            {
                Debug.Log($"item chain is null: {firstItem.ItemChain is null}");
                Debug.Log($"1. first item is stopped in chain: {firstItem.ItemChain.firstItemIsStopped.ToString()}");
                firstItem.ItemChain.SetItemCanMove();
                Debug.Log($"2. first item is stopped in chain: {firstItem.ItemChain.firstItemIsStopped.ToString()}");
                if (Input.GetKey(KeyCode.G)) Debug.Break();
            }*/
        }

        private BuiltObject GetObjectAtRotation(float rot, bool isForInputDirections)
        {
            if (!isForInputDirections) rot = MyMathf.CorrectAngle(rot + 180f);
            return rot switch
            {
                0f => topObject,
                90f => leftObject,
                180f => bottomObject,
                270f => rightObject,
                _ => null
            };
        }

        private void MakeInputPort(IInventory inv)
        {
            Transform thisTransform = transform;
            Vector3 thisPos = thisTransform.position;
            inputPortMade = true;
            Port.Create(inv, this, inputPortPrefab, new Vector3(thisPos.x, thisPos.y, -2f),
                Quaternion.Euler(0f, 0f, outputDirection), thisTransform);
        }

        private void MakeOutputPort(IInventory inv)
        {
            Transform thisTransform = transform;
            Vector3 thisPos = thisTransform.position;
            outputPortMade = true;
            Port.Create(inv, this, outputPortPrefab, new Vector3(thisPos.x, thisPos.y, -2f),
                Quaternion.Euler(0f, 0f, inputDirection), thisTransform);
        }

        /*private bool IsBeltBehind()
        {
            Vector3 thisPos = transform.position;
            return GridBuildingSystem.Instance.grid
                .GetGridObject(new Vector2(thisPos.x, thisPos.y) + MyMathf.GetBackwardVectorOffset(outputDirection))
                .GetBuiltObject() is BeltBehaviour belt && belt.GetOutputDirection() == outputDirection;
        }*/

        private void MakeCorner(Vector3 rot)
        {
            if (rot == transform.rotation.eulerAngles) return;
            if (!isCorner) thisAnimator.runtimeAnimatorController = cornerController;
            transform.rotation = Quaternion.Euler(rot);
            isCorner = true;
            bool hasDeletedItem = DeleteItemOnPrevBelt();
            SetDirections();
            thisAnimationSync.SyncAnimation();
            if (itemList.ToList().FirstOrDefault(i => i.DistanceFromCenter <= 0f) is {} item)
            {
                item.CorrectBackItemPos(inputDirection, pivotPos, !hasDeletedItem);
                if (prevBelt.GetItemList().FirstOrDefault() is { } firstItem)
                {
                    firstItem.ItemChain.CheckForMerging(item);
                }
            }
            SendBlockUpdate();
        }

        private bool DeleteItemOnPrevBelt()
        {
            //TODO: reimplement ItemChain.RemoveFromItemList to save an item from being deleted. Currently, I am forcing it to always delete an item so it splits the chain.
            if (lastPrevBelt is null || lastPrevBelt.GetItemList().FirstOrDefault() is not { /*DistanceFromCenter: >= 0.25f*/ } item) return false;
            //Debug.Log($"is going to delete item: {item.DistanceFromCenter.ToString()} is > 0.25: {(item.DistanceFromCenter > 0.25f).ToString()}", (ItemBehaviour)item);
            item.ItemChain.SetItemCanMove(); // temporary while it deletes item
            item.DestroySelf(lastPrevBelt);
            return true;
        }

        private void MakeStraight()
        {
            if (transform.rotation.eulerAngles.z == outputDirection) return;
            //bool shouldUpdateAgain = isCorner;
            isCorner = false;
            bool hasDeletedItem = DeleteItemOnPrevBelt();
            thisAnimator.runtimeAnimatorController = straightController;
            transform.rotation = Quaternion.Euler(0f, 0f, outputDirection);
            SetDirections();
            thisAnimationSync.SyncAnimation();

            if (itemList.ToList().FirstOrDefault(i => i.DistanceFromCenter <= 0f) is { } item)
            {
                item.CorrectBackItemPos(inputDirection, pivotPos, !hasDeletedItem);
                if (prevBelt.GetItemList().FirstOrDefault() is { } firstItem)
                {
                    firstItem.ItemChain.CheckForMerging(item);
                }
            }
            //if (shouldUpdateAgain) ReceiveBlockUpdate();
            SendBlockUpdate();
        }

        private void SetDirections()
        {
            Vector3 thisRotation = transform.rotation.eulerAngles;
            if (isCorner)
            {
                if (thisRotation.z % 180 == 0)
                {
                    inputDirection = MyMathf.CorrectAngle(thisRotation.x - thisRotation.z);
                    outputDirection = Math.Abs(thisRotation.y - 180f) <= 0f
                        ? MyMathf.CorrectAngle(thisRotation.z + 90f)
                        : MyMathf.CorrectAngle(thisRotation.z - 90f);
                }
                else
                {
                    inputDirection = MyMathf.CorrectAngle(thisRotation.z - thisRotation.y);
                    outputDirection = MyMathf.CorrectAngle(thisRotation.z - 90f);
                }
            }
            else
            {
                outputDirection = Mathf.Round(thisRotation.z);
                inputDirection = outputDirection;
            }
        }
    }
}