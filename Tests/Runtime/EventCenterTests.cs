using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using MAF.EventSystem; // Your namespace
using MAF.Singleton;   // Required by EventCenter

namespace MAF.Tests.Runtime
{
    public class EventCenterTests
    {
        private struct TestEvent { public int Value; }
        private bool _eventReceived;
        private int _receivedValue;

        [SetUp]
        public void SetUp()
        {
            _eventReceived = false;
            _receivedValue = 0;
            // Ensure a clean slate for EventCenter if it persists across tests (due to Singleton nature)
            // This might require a manual Reset method in EventCenter or careful unregistration.
            // For this example, we assume tests run in an order or context where this is managed,
            // or EventCenter.Instance will create a fresh one if no DontDestroyOnLoad instance exists from a previous test run.
            // A more robust setup would involve explicitly cleaning up listeners or the EventCenter instance.
            if (EventCenter.Instance != null)
            {
                 // Attempt to clean up previous test listeners if EventCenter instance persists
                 // This is tricky because EventDictionary is private static in the provided EventCenter.
                 // A Reset() method in EventCenter would be ideal for testing.
                 // For now, we rely on unregistering what we register in each test.
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Explicitly unregister to avoid interference between tests
            EventCenter.Instance?.Unregister<TestEvent>(HandleTestEvent);
        }

        void HandleTestEvent(TestEvent eventData)
        {
            _eventReceived = true;
            _receivedValue = eventData.Value;
        }

        [UnityTest]
        public IEnumerator EventCenter_RegistersAndTriggersEvent()
        {
            Assert.IsNotNull(EventCenter.Instance, "EventCenter instance should exist.");
            EventCenter.Instance.Register<TestEvent>(HandleTestEvent);
            EventCenter.Instance.Trigger(new TestEvent { Value = 42 });

            yield return null; // Wait a frame for event to propagate if needed (usually synchronous)

            Assert.IsTrue(_eventReceived, "Event was not received.");
            Assert.AreEqual(42, _receivedValue, "Received event value is incorrect.");
        }

        [UnityTest]
        public IEnumerator EventCenter_UnregistersEvent()
        {
            Assert.IsNotNull(EventCenter.Instance, "EventCenter instance should exist.");
            EventCenter.Instance.Register<TestEvent>(HandleTestEvent);
            EventCenter.Instance.Unregister<TestEvent>(HandleTestEvent);

            EventCenter.Instance.Trigger(new TestEvent { Value = 99 });

            yield return null;

            Assert.IsFalse(_eventReceived, "Event was received after unregistering.");
        }

        [UnityTest]
        public IEnumerator EventCenter_TriggerWithNoListeners()
        {
            Assert.IsNotNull(EventCenter.Instance, "EventCenter instance should exist.");
            // No listeners registered for ThisSpecificEvent
            Assert.DoesNotThrow(() => EventCenter.Instance.Trigger(new TestEvent { Value = 10 }),
                "Triggering an event with no listeners should not throw an exception.");
            yield return null;
            Assert.IsFalse(_eventReceived, "Event should not have been received by HandleTestEvent as it was for a different context/unregistered.");
        }
    }
}