using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    [Serializable]
    public sealed class AiStateMachine : AnimatorEventListener.IAnimatorEvent
    {
        public const string ANY_STATE = "%FROMANYSTATE%";

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

        private AiProperties properties;

        public delegate bool TransitionCondition(Transform transform, Transform target, AiStateMachine machine);

        #region Utilities

        public float DistanceToTarget => Vector3.Distance(transform.position, target.position);
        public float VerticalDistanceToTarget => Vector3.Distance(transform.position.Isolate(Utility.Axis.Y), target.position.Isolate(Utility.Axis.Y));
        public float HorizontalDistanceToTarget => Vector3.Distance(transform.position.Remove(Utility.Axis.Y), target.position.Remove(Utility.Axis.Y));

        #endregion

        public AiStateMachine(Transform transform)
        {
            this.transform = transform;
        }

        public void Tick()
        {
            if (!isInitialized)
                EnterState(stateDefault);

            if (transitions.ContainsKey(ANY_STATE))
            {
                foreach (Transition t in transitions[ANY_STATE])
                {
                    if (t.condition(transform, target, this))
                    {
                        t.onTransition?.Invoke(this);
                        EnterState(t.destinationState);
                    }
                }
            }

            foreach (Transition t in currentTransitions)
            {
                if (t.condition(transform, target, this))
                {
                    t.onTransition?.Invoke(this);
                    EnterState(t.destinationState);
                }
            }

            if (CurrentState != null)
            {
                CurrentState.properties = properties;
                CurrentState.OnTick();
            }
        }

        public void OnAnimationEvent(string id)
        {
            CurrentState?.OnAnimationEvent(id);
        }

        public void RegisterState(State state, string name)
        {
            if (states.ContainsKey(name))
                throw new ArgumentException($"State name {name} already exists when trying to register {state}");

            state.Machine = this;
            states.Add(name, state);
        }

        public void RegisterTransition(string source, string destination, TransitionCondition condition, Action<AiStateMachine> onTransition = null)
        {
            if (!transitions.ContainsKey(source))
                transitions.Add(source, new List<Transition>());

            transitions[source].Add(new Transition(destination, condition, onTransition));
        }

        public void SetDefaultState(string state)
        {
            stateDefault = state;
        }

        public void SetProperties(AiProperties properties)
            => this.properties = properties;

        public void EnterState(string state)
        {
            if (!states.ContainsKey(state))
            {
                Debug.LogWarning($"Attempted to enter unregistered state {state}");
                return;
            }

            isInitialized = true;

            if (CurrentState != null)
            {
                CurrentState.properties = properties;
                CurrentState.OnExit();
            }

            this.state = state;

            CurrentState = states[state];
            currentTransitions = transitions.ContainsKey(state) ? transitions[state] : null;

            if (CurrentState != null)
            {
                CurrentState.properties = properties;
                CurrentState.OnEnter();
            }
        }

        public void RegisterVisualizer(string name)
        {
#if UNITY_EDITOR
            //TODO
#endif
        }

        [PublicAPI]
        public abstract class State
        {
            public AiStateMachine Machine { get; set; }

            public AiProperties properties;

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

            public virtual void OnEnter()
            {
            }

            public virtual void OnExit()
            {
            }

            public abstract void OnTick();

            public virtual void OnAnimationEvent(string id)
            {
            }
        }

        [Serializable]
        public class AiProperties
        {
        }

        private struct Transition
        {
            public string destinationState;
            public TransitionCondition condition;
            public Action<AiStateMachine> onTransition;

            public Transition(string destinationState, TransitionCondition condition, Action<AiStateMachine> onTransition)
            {
                this.destinationState = destinationState;
                this.condition = condition;
                this.onTransition = onTransition;
            }
        }
    }
}