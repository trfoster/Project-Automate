using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectAutomate
{
    public static class TickSystem
    {
        public class OnTickEventArgs : EventArgs
        {
            public int Tick;
        }
        public static event EventHandler<OnTickEventArgs> OnAnimationTick;
        public static event EventHandler<OnTickEventArgs> BeforeAnimationTick;
        public static event EventHandler<OnTickEventArgs> OnFluidTick;
        private const float ANIMATION_TICK_TIMER_MAX = 0.03125f;
        private const float FLUID_TICK_TIMER_MAX = 0.016666666666666666666666666666f;
        private static GameObject tickSystemGameObject;
        private static int animationTick;
        private static int fluidTick;
        
        public static void Create()
        {
            if (tickSystemGameObject is not null) return;
            tickSystemGameObject = new GameObject("TickSystem");
            tickSystemGameObject.AddComponent<AnimationTickSystemObject>();
            tickSystemGameObject.AddComponent<FluidTickSystemObject>();
        }

        public static void Destroy()
        {
            Object.Destroy(tickSystemGameObject);
            OnAnimationTick = null;
            BeforeAnimationTick = null;
            OnFluidTick = null;
            tickSystemGameObject = null;
        }
        private class AnimationTickSystemObject : MonoBehaviour
        {
            private float tickTimer;

            private void Update()
            {
                tickTimer += Time.deltaTime;
                if (!(tickTimer >= ANIMATION_TICK_TIMER_MAX)) return;
                tickTimer -= ANIMATION_TICK_TIMER_MAX;
                animationTick++;
                BeforeAnimationTick?.Invoke(this, new OnTickEventArgs { Tick = animationTick });
                OnAnimationTick?.Invoke(this, new OnTickEventArgs { Tick = animationTick });
            }
        }

        private class FluidTickSystemObject : MonoBehaviour
        {
            private float tickTimer;

            private void Update()
            {
                tickTimer += Time.deltaTime;
                if (!(tickTimer >= FLUID_TICK_TIMER_MAX)) return;
                tickTimer -= FLUID_TICK_TIMER_MAX;
                fluidTick++;
                OnFluidTick?.Invoke(this, new OnTickEventArgs { Tick = fluidTick });
            }
        }
    }
}
