using System.Collections.Generic;
using System.Linq;
using TMechs.Environment.Targets;
using UnityEditor;
using UnityEngine;

namespace TMechs.PlayerOld
{
    public class TargetController : MonoBehaviour
    {
        public static TargetController Instance { get; private set; }

        public float range = 50F;
        public float angle = 45F;
        public float yRange = 20F;

        [HideInInspector]
        public bool isStopped;

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
            GetTarget<EnemyTarget>()?.Ping(currentTarget);
            GetTarget<GrappleTarget>()?.Ping(currentTarget);
        }

        private void FixedUpdate()
        {
            targetsInRange.Clear();

            foreach (BaseTarget target in REGISTERED_TARGETS)
            {
                if (!target || !target.CanTarget())
                    continue;

                if (IsInRange(target.transform.position))
                {
                    Vector3 heading = target.transform.position - transform.position;
                    heading = heading.Remove(Utility.Axis.Y);
                    float distance = heading.magnitude;
                    Vector3 direction = heading / distance;
                    
                    float angle = Vector3.Angle(direction, transform.forward.Remove(Utility.Axis.Y));
                    if(angle <= this.angle)
                        targetsInRange.Add(target);
                }
            }
        }

        public bool IsInRange(Vector3 point)
        {
            Vector3 origin = Player.Instance.transform.position;
            
            Vector3 heading = point - origin;
            float yDistance = Mathf.Abs(heading.y);
            heading = heading.Remove(Utility.Axis.Y);
            float distance = heading.magnitude;
            
            return distance <= range && yDistance <= yRange;
        }

        public BaseTarget GetTarget(System.Type type = null)
        {
            if (isStopped)
                return null;

            if (currentTarget && IsInRange(currentTarget.transform.position))
            {
                if (type != null && !type.IsInstanceOfType(currentTarget))
                    return null;
                
                return currentTarget;
            }

            currentTarget = null;

            IEnumerable<BaseTarget> targets = targetsInRange.Where(x => x);
            if (type != null)
                targets = targets.Where(x => type.IsInstanceOfType(x) && x.CanTarget());

            targets = targets
                    .OrderByDescending(x => x.GetPriority())
                    .ThenBy(x => Vector3.Distance(transform.position, x.transform.position));

            // Raycast to ensure we can see the target
            return targets.FirstOrDefault(x =>
            {
                Vector3 heading = x.transform.position + Vector3.up * .5F - transform.position;
                float distance = heading.magnitude;

                bool wasHit = Physics.Raycast(transform.position, heading / distance, out RaycastHit hit, distance, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore);

                return !wasHit || hit.rigidbody && hit.rigidbody.transform == x.transform || hit.transform == x.transform;
            });
        }

        public T GetTarget<T>() where T : BaseTarget
        {
            return (T) GetTarget(typeof(T));
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

            Vector3 angledPoint = Vector3.forward * range;
            Vector3 leftPoint = Quaternion.AngleAxis(angle, Vector3.up) * angledPoint;
            Vector3 rightPoint = Quaternion.AngleAxis(-angle, Vector3.up) * angledPoint;

            Vector3 offset = Vector3.up * yRange;

            // Lines from player outwards
            Gizmos.DrawLine(offset, leftPoint + offset);
            Gizmos.DrawLine(offset, rightPoint + offset);
            Gizmos.DrawLine(-offset, leftPoint - offset);
            Gizmos.DrawLine(-offset, rightPoint - offset);

            // Lines connecting the top and bottom
            Gizmos.DrawLine(offset, -offset);
            Gizmos.DrawLine(leftPoint + offset, leftPoint - offset);
            Gizmos.DrawLine(rightPoint + offset, rightPoint - offset);

            // Outer edge
            Vector3 halfLeft = Quaternion.AngleAxis(angle / 2F, Vector3.up) * angledPoint;
            Vector3 halfRight = Quaternion.AngleAxis(-angle / 2F, Vector3.up) * angledPoint;

            Gizmos.DrawLine(leftPoint + offset, halfLeft + offset);
            Gizmos.DrawLine(rightPoint + offset, halfRight + offset);
            Gizmos.DrawLine(leftPoint - offset, halfLeft - offset);
            Gizmos.DrawLine(rightPoint - offset, halfRight - offset);

            Gizmos.DrawLine(halfLeft + offset, angledPoint + offset);
            Gizmos.DrawLine(halfRight + offset, angledPoint + offset);
            Gizmos.DrawLine(halfLeft - offset, angledPoint - offset);
            Gizmos.DrawLine(halfRight - offset, angledPoint - offset);

            Gizmos.DrawLine(angledPoint + offset, angledPoint - offset);
            Gizmos.DrawLine(halfLeft + offset, halfLeft - offset);
            Gizmos.DrawLine(halfRight + offset, halfRight - offset);
        }
    }
}