using ProjectAutomate.Belts;
using UnityEngine;

namespace ProjectAutomate.Items
{
    //TODO: benchmark interface vs straight class as there is only one inheritor of this interface
    public interface IItem
    {
        float DistanceFromCenter { get; }
        ItemSO ItemSo { get; }
        void DestroySelf(BeltBehaviour beltUnderneath);
        ItemChain ItemChain { get; set; }
        bool UpdateItemMovement();
        void CorrectBackItemPos(float inputDirection, Vector2 beltPos, bool shouldRemoveFromList);
        bool IsFirstItem { get; set; }
        //void CheckForItemInFront();
        BeltBehaviour GetBeltUnderneath();
    }
}
