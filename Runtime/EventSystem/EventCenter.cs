// File: EventCenter.cs
using System;
using UnityEngine;
using System.Collections.Generic;
using MAF.Singleton; // Assuming your Singleton<T> is in this namespace

namespace MAF.EventSystem
{
    /// <summary>
    /// A centralized event management system.
    /// Allows for type-safe event registration, unregistration, and triggering.
    /// This class is a Singleton to provide global access.
    /// Note: This implementation is not inherently thread-safe for dictionary modifications
    /// if events are registered/unregistered from multiple threads simultaneously.
    /// For typical Unity main-thread usage, this is generally fine.
    /// </summary>
    public class EventCenter : Singleton<EventCenter>
    {
        /// <summary>
        /// Delegate for handling events with specific event data.
        /// The 'in' keyword allows for contravariance.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event data.</typeparam>
        /// <param name="eventData">The data associated with the event.</param>
        public delegate void EventHandler<in TEvent>(TEvent eventData);

        // Internal dictionary storing event types and their corresponding delegates.
        private readonly Dictionary<Type, Delegate> _eventDictionary = new Dictionary<Type, Delegate>();
        private readonly object _lock = new object(); // For thread-safe modifications to the dictionary

        /// <summary>
        /// Registers a handler for a specific event type.
        /// Multiple handlers can be registered for the same event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to register for.</typeparam>
        /// <param name="handler">The event handler method.</param>
        public void Register<TEvent>(EventHandler<TEvent> handler) where TEvent : struct // Or class, depending on your event data design
        {
            if (handler == null)
            {
                Debug.LogWarning($"[EventCenter] Attempted to register a null handler for event type {typeof(TEvent).Name}.");
                return;
            }

            Type eventType = typeof(TEvent);
            lock (_lock)
            {
                if (_eventDictionary.TryGetValue(eventType, out Delegate existingHandler))
                {
                    _eventDictionary[eventType] = Delegate.Combine(existingHandler, handler);
                }
                else
                {
                    _eventDictionary[eventType] = handler;
                }
            }
        }

        /// <summary>
        /// Unregisters a handler for a specific event type.
        /// The handler must be the same instance that was originally registered.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to unregister from.</typeparam>
        /// <param name="handler">The event handler method to remove.</param>
        public void Unregister<TEvent>(EventHandler<TEvent> handler) where TEvent : struct // Or class
        {
             if (handler == null)
            {
                Debug.LogWarning($"[EventCenter] Attempted to unregister a null handler for event type {typeof(TEvent).Name}.");
                return;
            }

            Type eventType = typeof(TEvent);
            lock (_lock)
            {
                if (_eventDictionary.TryGetValue(eventType, out Delegate existingHandler))
                {
                    Delegate newHandler = Delegate.Remove(existingHandler, handler);
                    if (newHandler == null)
                    {
                        _eventDictionary.Remove(eventType);
                    }
                    else
                    {
                        _eventDictionary[eventType] = newHandler;
                    }
                }
                else
                {
                     Debug.LogWarning($"[EventCenter] Attempted to unregister handler for event type {eventType.Name} but no handlers were registered for this type.");
                }
            }
        }

        /// <summary>
        /// Triggers an event of a specific type with the given event data.
        /// All registered handlers for this event type will be invoked.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to trigger.</typeparam>
        /// <param name="eventData">The data to pass to the event handlers.</param>
        public void Trigger<TEvent>(TEvent eventData) where TEvent : struct // Or class
        {
            Type eventType = typeof(TEvent);
            Delegate handlers;
            lock (_lock)
            {
                if (!_eventDictionary.TryGetValue(eventType, out handlers))
                {
                    // It's often fine not to have listeners, so a warning might be too noisy.
                    // Debug.Log($"[EventCenter] Triggered event {eventType.Name} but no listeners were registered.");
                    return;
                }
            }

            // Invoke outside the lock to prevent deadlocks if a handler tries to Register/Unregister.
            // Note: This means handlers might be invoked even if they were unregistered immediately after the lock was released.
            // This is a common trade-off.
            (handlers as EventHandler<TEvent>)?.Invoke(eventData);
        }
    }

    // Example of an event data structure:
    // public struct MyCustomEvent
    // {
    //     public int Value;
    //     public string Message;
    // }
}