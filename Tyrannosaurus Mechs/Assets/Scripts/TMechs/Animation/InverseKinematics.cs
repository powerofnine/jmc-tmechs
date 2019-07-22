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
        public Transform extendJoint;
        
        public Vector3 targetPosition;

        private Vector3 defaultPosition;
        private Quaternion[] defaultRotations;
        private Transform[] parents;

        private bool isReset;

        private void Awake()
        {
            defaultPosition = extendJoint.localPosition;
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

                for (int i = 0; i < aimJoints.Length; i++)
                {
                    aimJoints[i].localRotation = defaultRotations[i];
                }

                extendJoint.localPosition = defaultPosition;
                
                return;
            }

            weight = Mathf.Clamp(weight, 0F, .9F);

            isReset = false;

            OrientJoints();
            ExtendJoints();
        }

        private void OrientJoints()
        {
            foreach (Transform t in aimJoints)
                t.right = -(targetPosition - t.position).normalized;
        }

        private void ExtendJoints()
        {
            if (!extendJoint)
                return;
            
            Vector3 target = Vector3.Lerp(parents.Last().TransformPoint(defaultPosition), targetPosition, weight);
            extendJoint.position = target;
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