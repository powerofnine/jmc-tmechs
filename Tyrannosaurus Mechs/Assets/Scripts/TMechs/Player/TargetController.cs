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

        [HideInInspector]
        public bool isStopped = false;
        
        private EnemyTarget currentTarget;
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
                if (!target)
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

        public BaseTarget GetTarget(System.Type type = null)
        {
            if (isStopped)
                return null;
            
            if (currentTarget && targetsInRange.Contains(currentTarget))
                return currentTarget;
            currentTarget = null;

            IEnumerable<BaseTarget> targets = targetsInRange.Where(x => x);
            if (type != null)
                targets = targets.Where(type.IsInstanceOfType);

            targets = targets
                .OrderByDescending(x => x.GetPriority())
                .ThenBy(x => Vector3.Distance(transform.position, x.transform.position));

            // Raycast to ensure we can see the target
            return targets.FirstOrDefault(x =>
            {
                Vector3 heading = x.transform.position - transform.position;
                float distance = heading.magnitude;

                bool wasHit = Physics.Raycast(Player.Instance.Rigidbody.worldCenterOfMass, heading / distance, out RaycastHit hit, distance);
                
                return !wasHit || (hit.rigidbody && hit.rigidbody.transform == x.transform) || hit.transform == x.transform;
            });;
        }

        public T GetTarget<T>() where T : BaseTarget
        {
            return (T)GetTarget(typeof(T));
        }

        public EnemyTarget GetLock()
        {
            if (isStopped)
                return null;
            
            if (!targetsInRange.Contains(currentTarget))
                currentTarget = null;

            return currentTarget;
        }

        public EnemyTarget HardLock()
        {
            currentTarget = GetTarget<EnemyTarget>();
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