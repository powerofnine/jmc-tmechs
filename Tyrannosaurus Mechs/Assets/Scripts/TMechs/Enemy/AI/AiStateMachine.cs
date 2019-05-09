using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    [Serializable]
    public sealed class AiStateMachine
    {
        public const string ANY_STATE = "%FROMANYSTATE%";

        public bool tickWhenPaused = false;
        
        [SerializeField]
        private string state = "None";
        private string stateDefault;
        private bool isInitialized;

        public Transform transform;
        public Transform target;

        private readonly Dictionary<string, State> states = new Dictionary<string, State>();
        private readonly Dictionary<string, List<Transition>> transitions = new Dictionary<string, List<Transition>>();
        public State CurrentState { get; private set; }
        private List<Transition> currentTransitions;

        private readonly HashSet<string> triggers = new HashSet<string>();
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public delegate bool TransitionCondition(AiStateMachine machine);

        public object shared;

        #region Utilities

        public float DistanceToTarget => Vector3.Distance(transform.position, target.position);
        public float VerticalDistanceToTarget => Vector3.Distance(transform.position.Isolate(Utility.Axis.Y), target.position.Isolate(Utility.Axis.Y));
        public float HorizontalDistanceToTarget => Vector3.Distance(transform.position.Remove(Utility.Axis.Y), target.position.Remove(Utility.Axis.Y));
        public Vector3 DirectionToTarget => (target.position - transform.position).normalized;
        public Vector3 HorizontalDirectionToTarget => (target.position - transform.position).Remove(Utility.Axis.Y).normalized;
        public float AngleToTarget => Vector3.Angle(HorizontalDirectionToTarget, transform.forward.Remove(Utility.Axis.Y).normalized);

        #endregion

        public AiStateMachine(Transform transform)
        {
            this.transform = transform;
        }

        public void Tick()
        {
            if(!tickWhenPaused && Time.timeScale <= float.Epsilon)
                return;
            
            if (!isInitialized)
                EnterState(stateDefault);

#if UNITY_EDITOR
            if (!snapshot.initialized)
                BuildSnapshot();
#endif

            if (transitions.ContainsKey(ANY_STATE))
            {
                foreach (Transition t in transitions[ANY_STATE])
                {
                    if (t.condition(this))
                    {
                        t.onTransition?.Invoke(this);
                        EnterState(t.destinationState);
                    }
                }
            }

            foreach (Transition t in currentTransitions)
            {
                if (t.condition(this))
                {
                    t.onTransition?.Invoke(this);
                    EnterState(t.destinationState);
                }
            }

            UpdateSnapshot();

            CurrentState?.OnTick();
        }

        public void OnEvent(EventType type, string id)
        {
            CurrentState?.OnEvent(type, id);
        }

        public void RegisterState(State state, string name)
        {
            if (states.ContainsKey(name))
                throw new ArgumentException($"State name {name} already exists when trying to register {state}");

            if (state != null)
                state.Machine = this;
            states.Add(name, state);

            BuildSnapshot();
        }

        public void RegisterTransition(string source, string destination, TransitionCondition condition, Action<AiStateMachine> onTransition = null)
        {
            if (!transitions.ContainsKey(source))
                transitions.Add(source, new List<Transition>());

            transitions[source].Add(new Transition(destination, condition, onTransition));

            BuildSnapshot();
        }

        public void SetDefaultState(string state)
        {
            stateDefault = state;
        }

        public void EnterState(string state)
        {
            if (!states.ContainsKey(state))
            {
                Debug.LogWarning($"Attempted to enter unregistered state {state}");
                return;
            }

            isInitialized = true;

            CurrentState?.OnExit();

            this.state = state;

            CurrentState = states[state];
            currentTransitions = transitions.ContainsKey(state) ? transitions[state] : null;

            CurrentState?.OnEnter();
        }

        #region Properties

        public void SetTrigger(string name, bool active = true)
        {
            if (active)
                triggers.Add(name);
            else
                triggers.Remove(name);
        }

        public bool GetTrigger(string name, bool pop = true)
            => pop ? triggers.Remove(name) : triggers.Contains(name);

        public void Set<T>(string name, T value)
        {
            if (value == null)
                throw new ArgumentException($"Attempted to assign a null value to property: {name}");
            if (properties.ContainsKey(name) && !(properties[name] is T))
                throw new ArgumentException($"Type mismatch between the existing property {name} and the attempted assignment. {properties[name].GetType()} != {typeof(T)}");

            properties[name] = value;
        }

        public T Get<T>(string name)
        {
            if (!properties.ContainsKey(name))
            {
                Debug.LogWarning($"[Missing Property] Property {name} does not exist");
                return default;
            }

            if (!(properties[name] is T))
            {
                Debug.LogWarning($"[Type Mismatch] Attempted to get value {name} of type {typeof(T)} when real type is {properties[name].GetType()}");
                return default;
            }

            return (T) properties[name];
        }

        public T GetAddSet<T>(string name, int value)
        {
            return GetAddSet<T>(name, (float) value);
        }

        public T GetAddSet<T>(string name, float value)
        {
            if (!HasValue(name))
                Set(name, default(T));

            object ob = Get<object>(name);

            switch (ob)
            {
                case int i:
                    ob = i + (int) value;
                    break;
                case float f:
                    ob = f + value;
                    break;
                case double d:
                    ob = d + value;
                    break;
            }

            Set(name, ob);

            if (ob is T)
                return (T) ob;

            return default;
        }

        public bool HasValue(string name)
            => properties.ContainsKey(name);

        public void ImportProperties(object ob)
        {
            FieldInfo[] fields = ob.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
                Set(field.Name, field.GetValue(ob));
        }

        #endregion

        public void RegisterVisualizer(string name)
        {
#if UNITY_EDITOR
            AiGraphClient.RegisterMachine(name, this);
#endif
        }

        private void BuildSnapshot()
        {
#if UNITY_EDITOR
            snapshot = new MachineSnapshot();
            {
                snapshot.states = states.Select(x => x.Key).Prepend(ANY_STATE).ToArray();
                snapshot.currentState = state;

                List<Tuple<string, string>> transitionStates = new List<Tuple<string, string>>();
                foreach (KeyValuePair<string, List<Transition>> branch in transitions)
                foreach (Transition transition in branch.Value)
                    transitionStates.Add(new Tuple<string, string>(branch.Key, transition.destinationState));

                snapshot.transitions = transitionStates.ToArray();

                snapshot.initialized = true;
            }
#endif
        }

        private void UpdateSnapshot()
        {
#if UNITY_EDITOR
            snapshot.currentState = state;
#endif
        }

        [PublicAPI]
        public abstract class State
        {
            public AiStateMachine Machine { get; set; }

            // ReSharper disable once InconsistentNaming
            public Transform transform => Machine.transform;

            // ReSharper disable once InconsistentNaming
            public Transform target
            {
                get => Machine.target;
                set => Machine.target = value;
            }

            public float DistanceToTarget => Machine.DistanceToTarget;
            public float VerticalDistanceToTarget => Machine.VerticalDistanceToTarget;
            public float HorizontalDistanceToTarget => Machine.HorizontalDistanceToTarget;
            public Vector3 DirectionToTarget => Machine.DirectionToTarget;
            public Vector3 HorizontalDirectionToTarget => Machine.HorizontalDirectionToTarget;
            public float AngleToTarget => Machine.AngleToTarget;

            public virtual void OnEnter()
            {
            }

            public virtual void OnExit()
            {
            }

            public virtual void OnTick()
            {
            }

            public virtual void OnEvent(EventType type, string id)
            {
            }
        }

        public enum EventType
        {
            Animation,
            Trigger
        }

        private struct Transition
        {
            public readonly string destinationState;
            public readonly TransitionCondition condition;
            public readonly Action<AiStateMachine> onTransition;

            public Transition(string destinationState, TransitionCondition condition, Action<AiStateMachine> onTransition)
            {
                this.destinationState = destinationState;
                this.condition = condition;
                this.onTransition = onTransition;
            }
        }

        #region Snapshot

#if UNITY_EDITOR
        public MachineSnapshot snapshot;

        public struct MachineSnapshot
        {
            public bool initialized;

            public string[] states;
            public Tuple<string, string>[] transitions;

            public string currentState;

            public Vector2[] positions;
        }
#endif

        #endregion
    }
}