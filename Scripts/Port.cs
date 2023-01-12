using ProjectAutomate.Belts;
using ProjectAutomate.Inventories;
using UnityEngine;

namespace ProjectAutomate
{
    public class Port : MonoBehaviour
    {
        protected IInventory Inventory;
        protected BeltBehaviour Belt;
        public static void Create(IInventory inv, BeltBehaviour belt, Transform portPrefab, Vector3 pos, Quaternion rotation, Transform parent)
        {
            Transform thisTransform = Instantiate(portPrefab, pos, rotation, parent);
            Port thisPort = thisTransform.GetComponent<Port>();
            thisPort.Inventory = inv;
            thisPort.Belt = belt;
        }

        /*private InputPortBehaviour GetInputPort()
        {
            return GetComponent<InputPortBehaviour>();
        }
        
        private OutputPortBehaviour GetOutputPort()
        {
            return GetComponent<OutputPortBehaviour>();
        }*/
    }
}
