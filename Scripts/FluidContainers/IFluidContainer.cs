namespace ProjectAutomate.FluidContainers
{
    public interface IFluidContainer
    {
        float GetMaxContent();
        float GetContent();
        void SetContent(float content);
        float GetMoveLeft();
        float GetMoveTop();
    

    }
}
