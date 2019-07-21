using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace TMechs.Animation
{
    public class InverseKinematics : MonoBehaviour
    {
        //TODO proper IK math rather than just arranging joints in straight line

        [Range(0F, 1F)]
        public float weight = 0F;

        public Transform[] aimJoints;
        public Transform[] extendJoints;
        
        public Vector3 targetPosition;

        private Vector3[] defaultPositions;
        private Quaternion[] defaultRotations;
        private Transform[] parents;

        private bool isReset;

        private void Awake()
        {
            defaultPositions = extendJoints.Select(x => x.localPosition).ToArray();
            defaultRotations = aimJoints.Select(x => x.localRotation).ToArray();
            parents = aimJoints.Select(x => x.parent).ToArray();
        }

        private void LateUpdate()
        {
            if (weight <= Mathf.Epsilon && isReset || aimJoints == null || aimJoints.Length == 0)
                return;
            
            if (weight <= Mathf.Epsilon)
            {
                isReset = true;

                for (int i = 0; i < extendJoints.Length; i++)
                {
                    extendJoints[i].localPosition = defaultPositions[i];
                    extendJoints[i].localRotation = defaultRotations[i];
                }
                
                return;
            }

            isReset = false;

            Vector3 target = Vector3.Lerp(parents.Last().TransformPoint(defaultPositions.Last()), targetPosition, weight);

            for (int i = 0; i < aimJoints.Length; i++)
            {
                float localWeight = (float)i / aimJoints.Length;
                aimJoints[i].position = Vector3.Lerp(parents[i].TransformPoint(defaultPositions[i]), target, localWeight);
            }
            
            for (int i = 0; i < aimJoints.Length; i++)
            {
                Vector3 targetLook;

                if (i + 1 < aimJoints.Length)
                    targetLook = aimJoints[i + 1].position;
                else
                    targetLook = targetPosition;

                aimJoints[i].right = -(targetLook - aimJoints[i].position).normalized;
                
                float localWeight = (float)i / aimJoints.Length;
                aimJoints[i].position = Vector3.Lerp(parents[i].TransformPoint(defaultPositions[i]), target, localWeight);
            }
        }
        
        public void Transition(float time, float targetWeight, Action callback = null)
        {
            Stop();
            StartCoroutine(Transition_Do(time, targetWeight, callback));
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

        private IEnumerator Transition_Do(float time, float targetWeight, Action callback)
        {
            float t = 0F;

            float startWeight = weight;
            
            while (t <= time)
            {
                t += Time.deltaTime;
                weight = Mathf.Lerp(startWeight, targetWeight, t / time);

                yield return null;
            }
            
            callback?.Invoke();
        }
    }
}