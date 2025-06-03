using UnityEngine;
using MAF.FiniteStateMachine; // Your framework's namespace

namespace MAF.Samples
{
    // --- Define States ---
    public class IdleState : IState
    {
        private readonly PlayerControllerFSM _player;
        public IdleState(PlayerControllerFSM player) { _player = player; }

        public void OnEnter() { _player.Log("Entering Idle State"); _player.SetColor(Color.gray); }
        public void OnExecute() { if (Input.GetKeyDown(KeyCode.W)) _player.Fsm.SwitchState<WalkingState>(); }
        public void OnExit() { _player.Log("Exiting Idle State"); }
    }

    public class WalkingState : IState
    {
        private readonly PlayerControllerFSM _player;
        public WalkingState(PlayerControllerFSM player) { _player = player; }

        public void OnEnter() { _player.Log("Entering Walking State"); _player.SetColor(Color.green); }
        public void OnExecute() { _player.Move(Vector3.forward * Time.deltaTime * 5f); if (Input.GetKeyUp(KeyCode.W)) _player.Fsm.SwitchState<IdleState>(); }
        public void OnExit() { _player.Log("Exiting Walking State"); }
    }

    public class PlayerControllerFSM : MonoBehaviour
    {
        public FSM Fsm { get; private set; }
        private Renderer _renderer;

        void Start()
        {
            _renderer = GetComponent<Renderer>();
            Fsm = new FSM(this); // Pass this MonoBehaviour as the "owner"

            // Add states
            Fsm.AddState(new IdleState(this)); // Pass reference to this PlayerControllerFSM
            Fsm.AddState(new WalkingState(this));

            // Set initial state and start
            Fsm.SetInitialState<IdleState>();
            Fsm.StartFSM();
        }

        void Update()
        {
            Fsm.ExecuteState(); // Execute current state's logic
        }

        // Helper methods for states to call
        public void Log(string message) => Debug.Log($"[PlayerFSM]: {message}");
        public void SetColor(Color color) { if (_renderer != null) _renderer.material.color = color; }
        public void Move(Vector3 direction) { transform.Translate(direction); }
    }
}