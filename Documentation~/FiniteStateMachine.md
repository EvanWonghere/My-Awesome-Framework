# Finite State Machine (FSM)

The FSM module allows you to create and manage states for game entities (e.g., AI, player characters, UI elements).

## Core Classes
- `MAF.FiniteStateMachine.FSM`: The state machine manager.
- `MAF.FiniteStateMachine.IState`: The interface that all states must implement.

## `IState` Interface Methods
- `void OnEnter()`: Called when the FSM enters this state.
- `void OnExecute()`: Called every update cycle while this state is active.
- `void OnExit()`: Called when the FSM exits this state.

## `FSM` Class Usage

**1. Define Your States:**
Implement the `IState` interface for each state.
```csharp
public class PatrolState : IState
{
    private FSM _ownerFSM;
    public PatrolState(FSM owner) { _ownerFSM = owner; } // Example: passing FSM or agent
    public void OnEnter() { Debug.Log("Entering Patrol State"); }
    public void OnExecute() { /* Move character, check for player, etc. */ }
    public void OnExit() { Debug.Log("Exiting Patrol State"); }
}

public class ChaseState : IState 
{
    // ... implementation ...
}
```

**2. Initialize and Configure the FSM:**

Typically in a MonoBehaviour's Start method.
```csharp
public class EnemyAI : MonoBehaviour
{
    private FSM _myFSM;

    void Start()
    {
        _myFSM = new FSM(this); // 'this' can be the owner/agent passed to states

        _myFSM.AddState(new PatrolState(_myFSM)); // Pass FSM if states need it
        _myFSM.AddState(new ChaseState(/* ... */));
        // Or using generic AddState if states have parameterless constructors
        // _myFSM.AddState<PatrolState>();


        _myFSM.SetInitialState<PatrolState>();
        _myFSM.StartFSM(); // Enters the initial state
    }

    void Update()
    {
        _myFSM.ExecuteState(); // Call current state's OnExecute
    }

    // Example method for states to trigger transitions
    public void PlayerSpotted()
    {
        if (_myFSM.CurrentStateType != typeof(ChaseState))
        {
            _myFSM.SwitchState<ChaseState>();
        }
    }
}
```

**3. Switching States:**

Call `_myFSM.SwitchState<NewStateType>();` from within a state's `OnExecute` or from the FSM's owner.

## Key `FSM` Methods:
- `FSM(object owner = null)`: Constructor, optionally takes an owner object.
- `AddState(IState state)` / `AddState<TState>()`: Adds a state instance or type.
- `SetInitialState<TState>()`: Designates the starting state.
- `StartFSM()`: Calls `OnEnter()` on the initial state.
- `SwitchState<TState>()`: Transitions to a new state.
- `ExecuteState()`: Calls `OnExecute()` of the current state.
- `CurrentState (property)`: Gets the current `IState` object.
- `CurrentStateType (property)`: Gets the `Type` of the current state.
