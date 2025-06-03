using UnityEngine;
using MAF.EventSystem; // Your framework's namespace
using TMPro; // If using TextMeshPro for UI

namespace MAF.Samples
{
    // Define a sample event data structure
    public struct PlayerScoreChangedEvent
    {
        public int NewScore;
        public string PlayerName;
    }

    public class EventDemo : MonoBehaviour
    {
        public TextMeshProUGUI scoreText; // Assign in Inspector
        private int _currentScore = 0;

        void OnEnable()
        {
            // Register to the event
            EventCenter.Instance.Register<PlayerScoreChangedEvent>(HandlePlayerScoreChanged);
            Debug.Log("EventDemo: Registered for PlayerScoreChangedEvent.");
        }

        void OnDisable()
        {
            // Always unregister when the object is disabled or destroyed
            EventCenter.Instance?.Unregister<PlayerScoreChangedEvent>(HandlePlayerScoreChanged);
            Debug.Log("EventDemo: Unregistered from PlayerScoreChangedEvent.");
        }

        void Start()
        {
            if (scoreText != null)
                scoreText.text = "Score: 0 (Player1)";
        }

        void Update()
        {
            // Example: Trigger event on key press
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _currentScore += 10;
                EventCenter.Instance.Trigger(new PlayerScoreChangedEvent 
                { 
                    NewScore = _currentScore, 
                    PlayerName = "Player1" 
                });
                Debug.Log("EventDemo: Triggered PlayerScoreChangedEvent from Key Alpha1.");
            }
        }

        void HandlePlayerScoreChanged(PlayerScoreChangedEvent eventData)
        {
            Debug.Log($"EventDemo: Received PlayerScoreChangedEvent for {eventData.PlayerName}! New Score: {eventData.NewScore}");
            if (scoreText != null && eventData.PlayerName == "Player1")
            {
                scoreText.text = $"Score: {eventData.NewScore} ({eventData.PlayerName})";
            }
            _currentScore = eventData.NewScore; // Update local score if needed
        }
    }
}