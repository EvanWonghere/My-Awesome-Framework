// File: IState.cs
namespace MAF.FiniteStateMachine
{
    /// <summary>
    /// Interface for a state in a Finite State Machine (FSM).
    /// Defines the essential methods that any concrete state must implement.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the FSM enters this state.
        /// Use this for setup logic specific to this state.
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Called every frame or update cycle while this state is active.
        /// Use this for the main logic of the state.
        /// </summary>
        void OnExecute();

        /// <summary>
        /// Called when the FSM exits this state.
        /// Use this for cleanup logic specific to this state.
        /// </summary>
        void OnExit();
    }
}