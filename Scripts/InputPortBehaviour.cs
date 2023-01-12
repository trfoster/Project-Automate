using System.Collections.Generic;
using System.Linq;
using ProjectAutomate.Items;
using UnityEngine;

namespace ProjectAutomate
{
    public sealed class InputPortBehaviour : Port
    {
        private Transform thisTransform;
        private Vector3 thisPos;
        private void Awake()
        {
            TickSystem.BeforeAnimationTick += AnimationTickSystem_BeforeTick;
        }

        private void Start()
        {
            thisTransform = transform;
            thisPos = thisTransform.position;
        }

        private void AnimationTickSystem_BeforeTick(object sender, TickSystem.OnTickEventArgs eventArgs)
        {
            IEnumerable<IItem> beltItemList = Belt.GetItemList();
            foreach (IItem item in beltItemList.ToList().Where(item =>
                         item.DistanceFromCenter >= 0.25f && Inventory.TryStoreItems(item.ItemSo, 1)))
            {
                item.ItemChain.SetItemCanMove();
                item.DestroySelf(Belt);
            }

            /*if (beltItemList.ToList()
                .Any(item => item.DistanceFromCenter >= 0.25f && Inventory.TryStoreItems(item.ItemSo, 1)))
            {
                item.DestroySelf(Belt);
            }*/
        }

        private void OnDestroy()
        {
            TickSystem.BeforeAnimationTick -= AnimationTickSystem_BeforeTick;
        }
    }
}
