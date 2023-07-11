using RControlLib.Model;

namespace RControlLib
{
    /// <summary>
    /// Represents a RelayCard instance of the Conrad 8-Port relay-card
    /// </summary>
    public class RelayCard
    {
        /// <summary>
        /// The const number of card ports
        /// </summary>
        public const ushort NumberOfCardPorts = 8;

        private readonly Conrad8RelayCard card;
        private int cardCount = 1;

        /// <summary>
        /// How many cards were found during initialization
        /// </summary>
        public int? DetectedCardCount
        {
            get
            {
                if (card?.IsInitialized == true)
                    return cardCount;

                return null;
            }
        }

        /// <summary>
        /// Initializes the card using the given serial port
        /// </summary>
        /// <param name="port">The COM-port (e.g. COM 1)</param>
        public RelayCard(string port)
        {
            card = new Conrad8RelayCard(port);
            Initalize();
        }

        /// <summary>
        /// Initializes the card using port, baudRate and dataBits
        /// </summary>
        /// <param name="port">The COM-port (e.g. COM 1)</param>
        /// <param name="baudRate">The rate at which information is transferred in a communication channel.</param>
        /// <param name="dataBits">Bits per byte (normally 8)</param>
        public RelayCard(string port, ushort baudRate, ushort dataBits)
        {
            card = new Conrad8RelayCard(port, baudRate, dataBits);
            Initalize();
        }

        private void Initalize()
        {
            cardCount = card.InitializeCard();
        }

        /// <summary>
        /// Works like a taster, switches the relay on and off again
        /// </summary>
        /// <param name="relay">The relay index (0..7)</param>
        /// <param name="address">
        /// 0: Broadcast (all cards) <br />
        /// 1: First Card <br />
        /// 2: Second card ... <br />
        /// 255: Last card
        /// </param>
        /// <returns>The actual state of the relay card after switching</returns>
        public string SwitchButtonState(int relay, int address = 1)
        {
            string stateCurrent = ReadState(address);
            string on = string.Empty;
            string off = string.Empty;

            for (int i = 0; i < stateCurrent.Length; i++)
            {
                if (7 - i == relay)
                {
                    on += "1";
                    off += "0";
                }
                else
                {
                    on += stateCurrent[i];
                    off += stateCurrent[i];
                }
            }

            // Taster: [OFF]: ON => OFF
            SetState(on, address);
            return SetState(off, address);
        }

        /// <summary>
        /// Switches to the given state
        /// </summary>
        /// <param name="state">The state to switch to e.g. 01010101</param>
        /// <param name="address">
        /// 0: Broadcast (all cards) <br />
        /// 1: First Card <br />
        /// 2: Second card ... <br />
        /// 255: Last card
        /// </param>
        /// <returns>The actual state of the relay card after switching</returns>
        public string SetState(string state, int address = 1)
        {
            bool[] rawData = new bool[NumberOfCardPorts];

            // Length Validation
            if (state.Length > NumberOfCardPorts)
            {
                // Cut off
                state = state.Substring(0, NumberOfCardPorts);
            }
            else if (state.Length < NumberOfCardPorts)
            {
                // Add zeros
                while (state.Length < NumberOfCardPorts)
                    state += "0";
            }

            // Generating raw data for command to switch
            for (int b = 0; b < rawData.Length; b++)
                rawData[b] = (state[NumberOfCardPorts - 1 - b] == '1');

            // Put command together
            var status = new CardRelayState(NumberOfCardPorts, address);
            status.FromByte(rawData);

            // Send command to relay card
            card.SetPortCommand(status);

            // Finally output the new current state of the relay card
            return ReadState(address);
        }

        /// <summary>
        /// Reads the state of the given card
        /// </summary>
        /// <param name="address">
        /// 0: Broadcast (all cards) <br />
        /// 1: First Card <br />
        /// 2: Second card ... <br />
        /// 255: Last card
        /// </param>
        /// <returns>The actual state of the relay card</returns>
        public string ReadState(int address = 1)
        {
            var res = card.GetPortCommand(address, NumberOfCardPorts);
            string result = string.Empty;

            for (int i = res.RelayState.CardState.Length - 1; i >= 0; i--)
                result += res.RelayState.CardState[i] ? "1" : "0";

            return result;
        }

        /// <summary>
        /// Reads the states of the given cards
        /// </summary>
        /// <param name="cards">Which cards (indices) should be read</param>
        /// <returns>The actual state of the relay cards</returns>
        public string[] ReadStates(int[] cards)
        {
            string[] state = new string[cards.Length];
            
            for (int c = 0; c < cards.Length; c++)
                state[c] = ReadState(cards[c]);

            return state;
        }
    }
}