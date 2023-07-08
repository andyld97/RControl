using RControl.Model;
using System;

namespace RControl
{
    public class Program
    {
        private static Conrad8RelayCard relayCard = null;
        private static int numberOfCards = 0;

        /// <summary>
        /// 0 is Broadcast
        /// 1 is Card 1
        /// 2 is Card 2
        /// </summary>
        private const int FIRST_CARD = 1;

        /// <summary>
        /// Name of the serial port, e.g. COM1, /dev/ttyUSB0
        /// </summary>
        private static string DEFAULT_SERIAL_PORT_NAME = "/dev/ttyUSB0";

        private const char PARAM_HELP = 'h';
        private const char PARAM_CARD = 'c';
        private const char PARAM_PORT = 'p';
        private const char PARAM_SWITCH = 's';
        private const char PARAM_BUTTON = 't';
        private const char PARAM_RELAY = 'r';
        private const char PARAM_ALL = 'a';

        /// <summary>
        /// Usage:
        /// The documentation
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                DEFAULT_SERIAL_PORT_NAME = "/dev/ttyUSB0";
            else if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
                DEFAULT_SERIAL_PORT_NAME = "COM1";

            // Parse arguments
            if (args.Length == 1 && CheckParameter(args[0], PARAM_HELP))
            {
                // Print documentation
                PrintDocumentation();
                return;
            }

            try
            {
                // Init all cards at first
                InitalizeCards(CheckParameters(args, PARAM_PORT) ? ReadParameter(args, PARAM_PORT) : DEFAULT_SERIAL_PORT_NAME);

                // Now check other params
                if (args.Length == 0)
                {
                    // Read card 1
                    Console.Write(ReadCard(FIRST_CARD));
                    return;
                }
                else
                {
                    if (CheckParameters(args, PARAM_SWITCH))
                    {
                        bool cardIsSet = CheckParameters(args, PARAM_CARD);
                        string clearState = string.Empty;
                        for (int i = 0; i < Conrad8RelayCard.ConstNumberOfCardPorts; i++)
                            clearState += "0";

                        if (cardIsSet && !string.IsNullOrEmpty(ReadParameter(args, PARAM_CARD)))
                        {
                            // ToDo: Validate integer
                            SwitchRelayState(clearState, Convert.ToInt32(ReadParameter(args, PARAM_CARD)));
                        }
                        else
                        {
                            if (cardIsSet)
                            {
                                Console.WriteLine("FAIL: You need to add a card value");
                                return;
                            }

                            for (int c = 0; c < relayCard.DetectedCardCount; c++)
                                SwitchRelayState(clearState, c + 1);
                        }

                        return;
                    }
                    else if (CheckParameters(args, PARAM_BUTTON))
                    {
                        // Taster
                        if (CheckParameters(args, PARAM_RELAY) && !string.IsNullOrEmpty(ReadParameter(args, PARAM_RELAY)))
                        {
                            // ToDo: Validate integer
                            int relay = Convert.ToInt32(ReadParameter(args, PARAM_RELAY)) - 1;
                            int card = 1;

                            // Check if a card is also set
                            if (CheckParameters(args, PARAM_CARD)  && !string.IsNullOrEmpty(ReadParameter(args, PARAM_CARD)))
                            {
                                // Put card to value
                                int.TryParse(ReadParameter(args, PARAM_CARD), out card);
                            }

                            string stateCurrent = ReadCard(card);
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

                            // Tast
                            SwitchRelayState(on, card, true);
                            SwitchRelayState(off, card, false);                         
                        }
                        return;
                    }
                    else if (CheckParameters(args, PARAM_ALL))
                    {
                        // Workaround with string concat, because if I write after each read,
                        // QT C++ will get for each write-call a new output read. So the output is 8 Bits, but it repeats
                        // till DetectedcardCount
                        string finalResult = string.Empty;
                        for (int c = 0; c < relayCard.DetectedCardCount; c++)
                        {
                            int a = c + 1;
                            string stateCurrentOfAddress = ReadCard(a);

                            finalResult += stateCurrentOfAddress;
                        }

                        Console.Write(finalResult);
                        return;
                    }
                    else
                    {
                        int relay = -1;
                        if (int.TryParse(args[0], out relay) && args[0].Length <= 3)
                        {
                            // Read relay
                            Console.Write(ReadCard(relay));
                            return;
                        }
                        else
                        {
                            // Switching commands
                            if (args[0].Contains(","))
                            {
                                // Command is addressed to a single card
                                string[] commandData = args[0].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                                if (commandData.Length == 2)
                                {
                                    string switchData = commandData[1];
                                    int card = FIRST_CARD;

                                    if (int.TryParse(commandData[0], out card))
                                        SwitchRelayState(switchData, card);
                                    else
                                        Console.Write("The first char must be a number!");

                                    return;
                                }
                                else
                                {
                                    Console.Write("Format has to be C;XXXXXXXX and not " + args[0]);
                                    return;
                                }
                            }
                            else
                            {
                                if (args[0].Length == Conrad8RelayCard.ConstNumberOfCardPorts)
                                {
                                    // Command is addressed to the first card
                                    SwitchRelayState(args[0], FIRST_CARD);
                                    return;
                                }
                                else
                                {
                                    if (args[0].Length % Conrad8RelayCard.ConstNumberOfCardPorts == 0)
                                    {
                                        int cardsToAdress = args[0].Length / Conrad8RelayCard.ConstNumberOfCardPorts;

                                        if (cardsToAdress > relayCard.DetectedCardCount)
                                        {
                                            Console.WriteLine("Too many relays selected: {Need to address:" + cardsToAdress + "}, {DetectedCards:" + relayCard.DetectedCardCount + "}");
                                            return;
                                        }
                                        else
                                        {
                                            int currentCard = FIRST_CARD;
                                            string currentState = string.Empty;
                                            for (int r = 0; r < args[0].Length; r++)
                                            {
                                                int state = 0;
                                                if (int.TryParse(args[0][r].ToString(), out state))
                                                {
                                                    currentState += state;
                                                    SwitchRelayState(currentState, currentCard, true);

                                                    if ((r + 1) % Conrad8RelayCard.ConstNumberOfCardPorts == 0)
                                                    {
                                                        currentState = string.Empty;
                                                        currentCard++;
                                                    }
                                                }
                                                else
                                                {
                                                    Console.Write("Bitsequence contains illegal char!");
                                                    break;
                                                }
                                            }

                                            // ToDo: If this read is too heavy according to the performance return input??
                                            for (int c = 0; c < cardsToAdress; c++)
                                            {
                                                int a = c + 1;
                                                Console.Write(ReadCard(a));
                                            }
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid number of relays!");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Command syntax is wrong; please call help with RControl.exe -h!");
            }
            catch (Exception e)
            {
                Console.Write("ERROR: " + e.Message);
            }
        }

        private static void SwitchRelayState(string stateData, int adress, bool dontPrint = false)
        {
            bool[] rawData = new bool[Conrad8RelayCard.ConstNumberOfCardPorts];

            // Length Validation
            if (stateData.Length > Conrad8RelayCard.ConstNumberOfCardPorts)
            {
                // Cut off
                stateData = stateData.Substring(0, Conrad8RelayCard.ConstNumberOfCardPorts);
            }
            else if (stateData.Length < Conrad8RelayCard.ConstNumberOfCardPorts)
            {
                // Add zeros
                while (stateData.Length < Conrad8RelayCard.ConstNumberOfCardPorts)
                    stateData += "0";
            }

            // Generating raw data for command to switch
            for (int b = 0; b < rawData.Length; b++)
                rawData[b] = (stateData[Conrad8RelayCard.ConstNumberOfCardPorts -1 - b] == '1');

            // Put command together
            var state = new CardRelayState(Conrad8RelayCard.ConstNumberOfCardPorts, adress);
            state.FromByte(rawData);

            // Send command to relay card
            relayCard.SetPortCommand(state);

            // Finally output the new current state of the relay card
            if (!dontPrint)
                Console.Write(ReadCard(adress));
        }

        private static void InitalizeCards(string portName)
        {
            relayCard = new Conrad8RelayCard(portName);
            numberOfCards = relayCard.InitializeCard();
        }

        private static string ReadCard(int address)
        {
            var res = relayCard.GetPortCommand(address);
            string result = string.Empty;

            for (int i = res.RelayState.CardState.Length - 1; i >= 0; i--)
                result += res.RelayState.CardState[i] ? "1" : "0";

            return result;
        }

        private static bool CheckParameter(string param, char excepted)
        {
            string nParam = param.ToLower();
            return (nParam == $"/{excepted}" || nParam == $"-{excepted}");
        }

        private static bool CheckParameters(string[] values, char excepted)
        {
            foreach (string value in values)
            {
                if (CheckParameter(value, excepted))
                    return true;
            }

            return false;
        }

        private static string ReadParameter(string[] values, char excepted)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (CheckParameter(values[i], excepted) && i + 1 < values.Length)
                    return values[i + 1];
            }

            return string.Empty;
        }

        private static void PrintDocumentation()
        {
            string data =
                "Documentation for RelayControl.exe v1.0.0.2" + Environment.NewLine +
                "----------------------------------" + Environment.NewLine;

            Console.WriteLine(data);

            string[] commands = new string[] {
                "-h",
                "-p",
                "-s",
                "-s -c 1...255",
                "-t -r 1...8",
                "-t -c 1...255 -r 1...8",
                string.Empty,
                "Simple switching (must be always the first parameter)",
                "AAAAAAAA",
                "X,AAAAAAAA",
                "0,AAAAAAAA",
                "AAAAAAAABBBBBBBB",
                string.Empty,
                "Getting state",
                "-a",
                "1 ... 255",
                string.Empty
            };

            string[] explainations = new string[]
            {
                "Shows the documentation",
                "Port: Default port is /dev/ttyUSB0 for usage on Linux with Mono.",
                "Switch all relays off on each card",
                "Switch all relays off on Card X",
                "Handle Relay r on Card1 as a Taster (Switch on and after a delay off)",
                "Handle Relay r on Card c as a Taster (Switch on and after a delay off)",
                string.Empty,
                string.Empty,
                "A bit sequence which will be sent to Card 1",
                "A bit sequence which will be sent to Card X 1 <= X <= 255",
                "Broadcast: This command will be sent and executed by each and every card",
                "A bit sequence which will be splitted and each byte send to card x. x = 1; increments",
                string.Empty,
                string.Empty,
                "This command returns the state of all cards like AAAAAAAABBBBBBBB...",
                "This command returns the state of the selected card",
                "No command returns always the state of card 1"
            };

            var documentationTable = new ConsoleTables.ConsoleTable("Command", "Description");
            for (int i = 0; i < commands.Length; i++)
                documentationTable.AddRow(commands[i], explainations[i]);

            documentationTable.Write(ConsoleTables.Format.Alternative);
        }
    }
}