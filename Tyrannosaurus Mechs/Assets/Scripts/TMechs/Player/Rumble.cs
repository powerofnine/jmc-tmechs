using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

namespace TMechs.Player
{
    public class Rumble : MonoBehaviour
    {
        public const int CHANNEL_CAMERA = 0;
        public const int CHANNEL_ATTACK = 1;
        public const int CHANNEL_DAMAGED = 2;
        
        private static Rumble instance;
        
        private MotorSet[] motors;
        private readonly Dictionary<int, Channel> channels = new Dictionary<int, Channel>();
        
        private void Start()
        {
            Rewired.Player input = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);

            motors = input.controllers.Joysticks.Where(j => j.supportsVibration).Select(j => new MotorSet(j)).ToArray();
        }

        private void Update()
        {
            float output = 0F;

            foreach (Channel c in channels.Values)
            {
                if (c.time > 0F)
                {
                    float rumble = c.strength;
                    rumble = Mathf.Lerp(0F, rumble, (c.startTime - c.time) / c.fade);
                    rumble = Mathf.Lerp(rumble, 0F, c.time / c.fade);

                    if (rumble > output)
                        output = rumble;

                    c.time -= Time.deltaTime;
                }
            }
            
            foreach(MotorSet set in motors)
                set.Set(output);
        }

        public static void SetRumble(int channel, float strength, float time, float fade)
        {
            instance._SetRumble(channel, strength, time, fade);
        }

        private void _SetRumble(int channel, float strength, float time, float fade)
        {
            if(!channels.ContainsKey(channel))
                channels.Add(channel, new Channel());

            channels[channel].strength = strength;
            channels[channel].time = time;
            channels[channel].startTime = time;
            channels[channel].fade = fade;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject go = new GameObject("Rumble Support");
            instance = go.AddComponent<Rumble>();
        }

        private struct MotorSet
        {
            private Joystick joystick;
            private int motors;

            public MotorSet(Joystick joystick)
            {
                this.joystick = joystick;
                motors = joystick.vibrationMotorCount;
            }
            
            public void Set(float value)
            {
                for (int i = 0; i < motors; i++)
                    joystick.SetVibration(i, value, .25F, false);
            }
        }

        private class Channel
        {
            public float strength;
            public float time;
            public float startTime;
            public float fade;
        }
    }
}