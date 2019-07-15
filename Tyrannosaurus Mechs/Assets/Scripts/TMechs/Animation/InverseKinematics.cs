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

        public Transform[] orderedJoints;
        public Vector3 targetPosition;

        private Vector3[] defaultPositions;
        private Quaternion[] defaultRotations;
        private Transform[] parents;

        private bool isReset;

        private void Awake()
        {
            defaultPositions = orderedJoints.Select(x => x.localPosition).ToArray();
            defaultRotations = orderedJoints.Select(x => x.localRotation).ToArray();
            parents = orderedJoints.Select(x => x.parent).ToArray();
        }

        private void LateUpdate()
        {
            if (weight <= Mathf.Epsilon && isReset || orderedJoints == null || orderedJoints.Length == 0)
                return;
            
            if (weight <= Mathf.Epsilon)
            {
                isReset = true;

                for (int i = 0; i < orderedJoints.Length; i++)
                {
                    orderedJoints[i].localPosition = defaultPositions[i];
                    orderedJoints[i].localRotation = defaultRotations[i];
                }
                
                return;
            }

            isReset = false;

            Vector3 target = Vector3.Lerp(parents.Last().TransformPoint(defaultPositions.Last()), targetPosition, weight);

            for (int i = 0; i < orderedJoints.Length; i++)
            {
                float localWeight = (float)i / orderedJoints.Length;
                orderedJoints[i].position = Vector3.Lerp(parents[i].TransformPoint(defaultPositions[i]), target, localWeight);
            }
            
            for (int i = 0; i < orderedJoints.Length; i++)
            {
                Vector3 targetLook;

                if (i + 1 < orderedJoints.Length)
                    targetLook = orderedJoints[i + 1].position;
                else
                    targetLook = targetPosition;

                orderedJoints[i].right = -(targetLook - orderedJoints[i].position).normalized;
                
                float localWeight = (float)i / orderedJoints.Length;
                orderedJoints[i].position = Vector3.Lerp(parents[i].TransformPoint(defaultPositions[i]), target, localWeight);
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