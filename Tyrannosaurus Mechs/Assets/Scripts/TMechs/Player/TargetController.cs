using System.Collections.Generic;
using System.Linq;
using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player
{
    public class TargetController : MonoBehaviour
    {
        public static TargetController Instance { get; private set; }
        
        public Bounds box;

        private CamLockTarget currentTarget;
        private readonly HashSet<BaseTarget> targetsInRange = new HashSet<BaseTarget>();
        
        private static readonly HashSet<BaseTarget> REGISTERED_TARGETS = new HashSet<BaseTarget>();

        private void Awake()
        {
            Instance = this;
            
            InvokeRepeating(nameof(GC), 0F, 2F);
        }

        private void Update()
        {
            GetTarget()?.Ping(currentTarget);
        }

        private void FixedUpdate()
        {
            targetsInRange.Clear();

            foreach (BaseTarget target in REGISTERED_TARGETS)
            {
                if(!target)
                    continue;
                
                Vector3 point = transform.InverseTransformPoint(target.transform.position) - box.center;
                Vector3 extents = box.extents;

                //Approximate, but close enough
                if (point.x < extents.x && point.x > -extents.x &&
                    point.y < extents.y && point.y > -extents.y &&
                    point.z < extents.z && point.z > -extents.z)
                    targetsInRange.Add(target);
            }
        }

        public BaseTarget GetTarget(bool requireLock = false)
        {
            if (currentTarget && targetsInRange.Contains(currentTarget))
                return currentTarget;
            currentTarget = null;

            IEnumerable<BaseTarget> targets = targetsInRange.Where(x => x);
            if (requireLock)
                targets = targets.Where(x => x is CamLockTarget);

            targets = targets
                    .OrderByDescending(x => x.GetPriority())
                    .ThenBy(x => Vector3.Distance(transform.position, x.transform.position));

            return targets.FirstOrDefault();
        }

        public CamLockTarget GetLock()
        {
            if (!targetsInRange.Contains(currentTarget))
                currentTarget = null;
            
            return currentTarget;
        }

        public CamLockTarget HardLock()
        {
            currentTarget = (CamLockTarget)GetTarget(true);
            return currentTarget;
        }

        public void Unlock()
        {
            currentTarget = null;
        }

        // ReSharper disable once InconsistentNaming
        private void GC()
        {
            REGISTERED_TARGETS.RemoveWhere(x => !x);
        }

        public static void Add(BaseTarget target)
        {
            REGISTERED_TARGETS.Add(target);
        }

        public static void Remove(BaseTarget target)
        {
            REGISTERED_TARGETS.Remove(target);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}