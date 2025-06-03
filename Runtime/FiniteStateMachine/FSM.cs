// File: FSM.cs
using System;
using System.Collections.Generic;
using UnityEngine; // For Debug.LogWarning

namespace MAF.FiniteStateMachine
{
    /// <summary>
    /// A Finite State Machine (FSM) that manages a collection of states and transitions between them.
    /// </summary>
    public class FSM
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
        private IState _currentState;
        private bool _isTransitioning = false; // Prevents re-entrant state changes

        /// <summary>
        /// Gets the currently active state.
        /// </summary>
        public IState CurrentState => _currentState;

        /// <summary>
        /// Gets the type of the currently active state.
        /// </summary>
        public Type CurrentStateType => _currentState?.GetType();
        
        /// <summary>
        /// Optional: Reference to the agent (e.g., GameObject, character controller) that this FSM controls.
        /// States can use this to interact with the agent.
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FSM"/> class.
        /// </summary>
        /// <param name="owner">Optional: The agent or object that this FSM will control. 
        /// States can access this owner to perform actions or get data.</param>
        public FSM(object owner = null)
        {
            Owner = owner;
        }

        /// <summary>
        /// Adds a state instance to the FSM.
        /// </summary>
        /// <param name="state">The state instance to add. The FSM will use the instance's type as its key.</param>
        public void AddState(IState state)
        {
            if (state == null)
            {
                Debug.LogWarning("[FSM] Cannot add a null state.");
                return;
            }

            var type = state.GetType();
            if (!_states.TryAdd(type, state))
            {
                Debug.LogWarning($"[FSM] State of type {type.Name} already exists and was not added again.");
            }
        }

        /// <summary>
        /// Adds a state of a specific type to the FSM. The state will be instantiated using its default constructor.
        /// </summary>
        /// <typeparam name="TState">The type of the state to add. Must implement IState and have a parameterless constructor.</typeparam>
        public void AddState<TState>() where TState : IState, new()
        {
            var type = typeof(TState);
            if (_states.ContainsKey(type))
            {
                Debug.LogWarning($"[FSM] State of type {type.Name} already exists and was not added again.");
                return;
            }
            _states.Add(type, new TState());
        }
        
        /// <summary>
        /// Sets the initial state of the FSM. This does not call OnEnter for the state;
        /// use SwitchState for that after adding all states, or call StartFSM.
        /// </summary>
        /// <typeparam name="TState">The type of the state to set as initial.</typeparam>
        public void SetInitialState<TState>() where TState : IState
        {
            var type = typeof(TState);
            if (_states.TryGetValue(type, out var state))
            {
                _currentState = state;
            }
            else
            {
                Debug.LogError($"[FSM] Initial state {type.Name} not found. Ensure it has been added to the FSM.");
            }
        }

        /// <summary>
        /// Starts the FSM by entering its currently set initial state.
        /// Call this after adding all states and setting an initial state.
        /// </summary>
        public void StartFSM()
        {
            if (_currentState == null && _states.Count > 0)
            {
                 Debug.LogWarning("[FSM] FSM started without an explicit initial state. Consider using SetInitialState<T>(). Attempting to use the first added state (order not guaranteed).");
                 // Fallback: use the first state in the dictionary if no initial state was set.
                 // This is not ideal as dictionary order is not guaranteed.
                 // foreach(var state in _states.Values) { _currentState = state; break; }
                 // Better to require SetInitialState or an initial state parameter in constructor.
                 if(_currentState == null)
                 {
                    Debug.LogError("[FSM] Cannot start FSM. No initial state set and no states available.");
                    return;
                 }
            }
            
            if(_currentState != null)
            {
                Debug.Log($"[FSM] Starting FSM. Initial state: {_currentState.GetType().Name}");
                _currentState.OnEnter();
            }
            else
            {
                Debug.LogError("[FSM] Cannot start FSM. No initial state has been set or no states have been added.");
            }
        }


        /// <summary>
        /// Transitions the FSM to a new state of the specified type.
        /// Calls OnExit on the current state and OnEnter on the new state.
        /// </summary>
        /// <typeparam name="TState">The type of the state to transition to.</typeparam>
        public void SwitchState<TState>() where TState : IState
        {
            if (_isTransitioning)
            {
                Debug.LogWarning($"[FSM] Attempted to switch state to {typeof(TState).Name} while already transitioning. Request ignored.");
                return;
            }

            var type = typeof(TState);
            if (!_states.TryGetValue(type, out var newState))
            {
                Debug.LogWarning($"[FSM] State {type.Name} does not exist in the FSM. Switch aborted.");
                return;
            }

            if (_currentState == newState && _currentState != null) // Allow switching to the same state if current state is null (initial switch)
            {
                Debug.Log($"[FSM] Already in state {type.Name}. Re-entering state.");
                // Optionally, re-call OnEnter or have a specific ReEnter method
                _isTransitioning = true;
                _currentState?.OnExit();
                _currentState = newState;
                _currentState.OnEnter();
                _isTransitioning = false;
                return;
            }
            
            _isTransitioning = true;
            // Debug.Log($"[FSM] Switching state from {(_currentState?.GetType().Name ?? "None")} to {type.Name}");
            _currentState?.OnExit();
            _currentState = newState;
            _currentState.OnEnter();
            _isTransitioning = false;
        }

        /// <summary>
        /// Executes the OnExecute method of the current active state.
        /// This should be called regularly (e.g., in an Update loop).
        /// </summary>
        public void ExecuteState()
        {
            if (_isTransitioning) return; // Don't execute during a transition

            _currentState?.OnExecute();
        }
    }
}