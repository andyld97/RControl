using RControlLib.Model;

namespace RControlLib
{
    public class RelayCard
    {
        /// <summary>
        /// The const number of card ports
        /// </summary>
        public const ushort NumberOfCardPorts = 8;

        private readonly Conrad8RelayCard card;
        private int cardCount = 1;

        public int? DetectedCardCount
        {
            get
            {
                if (card?.IsInitialized == true)
                    return cardCount;

                return null;
            }
        }

        public RelayCard(string port)
        {
            card = new Conrad8RelayCard(port);
            Initalize();
        }

        public RelayCard(string port, ushort baudRate, ushort dataBits)
        {
            card = new Conrad8RelayCard(port, baudRate, dataBits);
            Initalize();
        }

        private void Initalize()
        {
            cardCount = card.InitializeCard();
        }

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

        public string ReadState(int address = 1)
        {
            var res = card.GetPortCommand(address, NumberOfCardPorts);
            string result = string.Empty;

            for (int i = res.RelayState.CardState.Length - 1; i >= 0; i--)
                result += res.RelayState.CardState[i] ? "1" : "0";

            return result;
        }

        public string[] ReadStates(int[] cards)
        {
            string[] state = new string[cards.Length];
            
            for (int c = 0; c < cards.Length; c++)
                state[c] = ReadState(cards[c]);

            return state;
        }
    }
}