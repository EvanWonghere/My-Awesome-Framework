# Event System (EventCenter)

The Event System provides a centralized way to manage game events, allowing different parts of your game to communicate without direct dependencies. It uses a `Singleton` instance called `EventCenter`.

## Core Class
`MAF.EventSystem.EventCenter`

## Features
- Type-safe event registration and triggering.
- Supports multiple handlers for a single event type.
- Global access via Singleton pattern.

## Usage

**1. Define an Event Data Structure (usually a struct):**
```csharp
public struct PlayerKilledEvent
{
    public int VictimID;
    public int AttackerID;
    public string WeaponUsed;
}
```

**2. Registering an Event Handler:**

Typically in `OnEnable` or an initialization method:
```csharp
void OnEnable()
{
    EventCenter.Instance.Register<PlayerKilledEvent>(HandlePlayerKilled);
}

void HandlePlayerKilled(PlayerKilledEvent eventData)
{
    Debug.Log($"Player {eventData.VictimID} was killed by {eventData.AttackerID} using {eventData.WeaponUsed}!");
}
```

**3. Triggering an Event:**
```csharp
// When a player is killed:
EventCenter.Instance.Trigger(new PlayerKilledEvent 
{ 
    VictimID = 1, 
    AttackerID = 2, 
    WeaponUsed = "Sword" 
});
```

**4. Unregistering an Event Handler:**

Crucial to prevent memory leaks, typically in OnDisable or OnDestroy:
```csharp
void OnDisable()
{
    EventCenter.Instance?.Unregister<PlayerKilledEvent>(HandlePlayerKilled);
}
```

## Important Notes
- **Thread Safety:** The provided `EventCenter`'s dictionary operations are wrapped in a `lock` for basic thread safety during registration/unregistration. Event invocation happens outside the lock to prevent deadlocks if handlers themselves try to modify registrations.
- **Manual Unregistration:** Always unregister your event handlers when the listening object is no longer active or is destroyed to avoid errors and memory leaks.