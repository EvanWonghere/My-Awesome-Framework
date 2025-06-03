using NUnit.Framework;
using MAF.FiniteStateMachine; // Your namespace

namespace MAF.Tests.Runtime
{
    public class FSMTests
    {
        private FSM _fsm;
        private MockStateA _stateA;
        private MockStateB _stateB;

        private class MockState : IState
        {
            public bool Entered { get; private set; }
            public bool Executed { get; private set; }
            public bool Exited { get; private set; }
            public virtual void OnEnter() { Entered = true; }
            public virtual void OnExecute() { Executed = true; }
            public virtual void OnExit() { Exited = true; }
            public void Reset() { Entered = false; Executed = false; Exited = false; }
        }
        private class MockStateA : MockState { }
        private class MockStateB : MockState { }
        private class UnaddedState : MockState { }


        [SetUp]
        public void SetUp()
        {
            _fsm = new FSM("TestOwner");
            _stateA = new MockStateA();
            _stateB = new MockStateB();

            _fsm.AddState(_stateA);
            _fsm.AddState(_stateB);
        }

        [Test]
        public void FSM_AddsState()
        {
            _fsm.SetInitialState<MockStateA>(); // Set an initial state to check
            _fsm.StartFSM();
            Assert.IsNotNull(_fsm.CurrentState);
            Assert.IsInstanceOf<MockStateA>(_fsm.CurrentState);
        }

        [Test]
        public void FSM_SetInitialStateAndStart()
        {
            _fsm.SetInitialState<MockStateA>();
            _fsm.StartFSM();
            Assert.IsTrue(_stateA.Entered, "Initial state OnEnter was not called.");
            Assert.IsFalse(_stateA.Exited, "Initial state OnExit should not have been called yet.");
        }


        [Test]
        public void FSM_SwitchesState()
        {
            _fsm.SetInitialState<MockStateA>();
            _fsm.StartFSM(); // Calls OnEnter for A
            _stateA.Reset(); // Reset flags after initial OnEnter

            _fsm.SwitchState<MockStateB>();

            Assert.IsTrue(_stateA.Exited, "Previous state (A) OnExit was not called.");
            Assert.IsInstanceOf<MockStateB>(_fsm.CurrentState, "FSM did not switch to the new state (B).");
            Assert.IsTrue(_stateB.Entered, "New state (B) OnEnter was not called.");
        }

        [Test]
        public void FSM_ExecutesCurrentState()
        {
            _fsm.SetInitialState<MockStateA>();
            _fsm.StartFSM();
            _fsm.ExecuteState();
            Assert.IsTrue(_stateA.Executed, "Current state OnExecute was not called.");
        }

        [Test]
        public void FSM_SwitchToNonExistentState_LogsWarningAndRemainsInCurrent()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, "[FSM] State UnaddedState does not exist in the FSM. Switch aborted.");
            _fsm.SetInitialState<MockStateA>();
            _fsm.StartFSM();

            IState originalState = _fsm.CurrentState;
            _fsm.SwitchState<UnaddedState>(); // This state was not added

            Assert.AreSame(originalState, _fsm.CurrentState, "FSM should remain in the current state if switch target doesn't exist.");
            Assert.IsInstanceOf<MockStateA>(_fsm.CurrentState);
        }

        [Test]
        public void FSM_AddExistingStateType_LogsWarning()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, "[FSM] State of type MockStateA already exists and was not added again.");
            var anotherStateA = new MockStateA();
            _fsm.AddState(anotherStateA); // Attempt to add another instance of MockStateA
        }
    }
}