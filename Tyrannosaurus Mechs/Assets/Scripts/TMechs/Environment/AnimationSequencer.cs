using System;
using Animancer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMechs.Environment
{
    public class AnimationSequencer : MonoBehaviour
    {
        public AnimationClip backgroundClip;
        public AnimationClip[] clips = { };

        private AnimancerComponent animancer;

        public bool randomizeDelayBetweenClips;
        [ConditionalHide("randomizeDelayBetweenClips", true, true)]
        public float delayBetweenClips;
        [ConditionalHide("randomizeDelayBetweenClips", true, false)]
        public float minDelayBetweenClips;
        [ConditionalHide("randomizeDelayBetweenClips", true, false)]
        public float maxDelayBetweenClips;

        private float timer;
        private bool timerRunning = true;
        private int clip;

        private AnimancerState current;

        private void Awake()
        {
            animancer = GetComponent<AnimancerComponent>();

            if (clips == null || clips.Length == 0 || !animancer)
                Destroy(this);

            timer = GetTimerValue();

            if (backgroundClip)
                animancer.Play(backgroundClip, 0);
        }

        private void Update()
        {
            if (!timerRunning)
                return;

            timer -= Time.deltaTime;
            if (timer <= 0F)
            {
                timerRunning = false;
                current = animancer.CrossFadeFromStart(clips[clip], 0F, 1);
                current.OnEnd = OnEnd;
            }
        }

        private void OnEnd()
        {
            animancer.GetLayer(1).StartFade(0F, .1F);
            current.OnEnd = null;

            timerRunning = true;
            timer = GetTimerValue();

            clip++;
            if (clip >= clips.Length)
                clip = 0;
        }

        private float GetTimerValue()
        {
            return !randomizeDelayBetweenClips ? delayBetweenClips : Random.Range(minDelayBetweenClips, maxDelayBetweenClips);
        }
    }
}