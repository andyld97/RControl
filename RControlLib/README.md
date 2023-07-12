## RControlLib - Usage
```cs   
// Initialize your relay card 
RelayCard card = new RelayCard("COM5");

// Set a new state (returns the read state from the card)
var state = card.SetState("00010011");

// Just switch a single relay ON (the first) [0..7]
state = card.Switch(0, true);

// Just read the state of the first card
state = card.ReadState();

// Card indices:
// 0: Broadcast (=ALL Cards)
// 1: First card
// 2: Second card
// 255: Last card
var secondState = card.ReadState(2);

// Detect how many cards are found
int count = card.DetectedCardCount;

// Read states of a set of cards (1..5)
var states = card.ReadStates(new[] { 1, 2, 3, 4, 5 });

// Taster (Relay 1 on first card [0..7]) 
// (shortly on and then off, like an impulse)
state = card.SwitchButtonState(0);

// Taster (Relay 8 on second card) 
state = card.SwitchButtonState(8, 2);
```