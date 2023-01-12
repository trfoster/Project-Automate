using System;
using UnityEngine;

namespace ProjectAutomate
{
    public sealed class AnimationSync : MonoBehaviour
    {
        private Animator thisAnimator;
        private Animator animSync;

        private void Awake()
        {
            animSync = GameHandler.AnimSyncAnimator;
            thisAnimator = GetComponent<Animator>();
        }

        private void Start()
        {
            SyncAnimation();
        }

        public void SyncAnimation()
        {
            thisAnimator.Play(0, -1, animSync.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
    }
}
