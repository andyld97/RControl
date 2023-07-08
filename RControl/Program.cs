using RControlLib;
using System;

namespace RControl
{
    public class Program
    {
        private static RelayCard relayCard = null;
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
                    Console.Write(relayCard.ReadState(FIRST_CARD));
                    return;
                }
                else
                {
                    if (CheckParameters(args, PARAM_SWITCH))
                    {
                        bool cardIsSet = CheckParameters(args, PARAM_CARD);
                        string clearState = string.Empty;
                        for (int i = 0; i < RelayCard.NumberOfCardPorts; i++)
                            clearState += "0";

                        if (cardIsSet && !string.IsNullOrEmpty(ReadParameter(args, PARAM_CARD)))
                        {
                            // ToDo: Validate integer
                            Console.WriteLine(relayCard.SetState(clearState, Convert.ToInt32(ReadParameter(args, PARAM_CARD))));
                        }
                        else
                        {
                            if (cardIsSet)
                            {
                                Console.WriteLine("FAIL: You need to add a card value");
                                return;
                            }

                            for (int c = 0; c < relayCard.DetectedCardCount; c++)
                                relayCard.SetState(clearState, c + 1);
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

                            Console.WriteLine(relayCard.SwitchButtonState(relay, card));                     
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
                            string stateCurrentOfAddress = relayCard.ReadState(a);

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
                            Console.Write(relayCard.ReadState(relay));
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
                                        relayCard.SetState(switchData, card);
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
                                if (args[0].Length == RelayCard.NumberOfCardPorts)
                                {
                                    // Command is addressed to the first card
                                    Console.WriteLine(relayCard.SetState(args[0], FIRST_CARD));
                                    return;
                                }
                                else
                                {
                                    if (args[0].Length % RelayCard.NumberOfCardPorts == 0)
                                    {
                                        int cardsToAdress = args[0].Length / RelayCard.NumberOfCardPorts;

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
                                                    Console.WriteLine(relayCard.SetState(currentState, currentCard));

                                                    if ((r + 1) % RelayCard.NumberOfCardPorts == 0)
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
                                                Console.Write(relayCard.ReadState(a));
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

        private static void InitalizeCards(string portName)
        {
            relayCard = new RelayCard(portName);
            numberOfCards = relayCard.DetectedCardCount ?? 0;
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
                "Documentation for RelayControl.exe v1.0.3" + Environment.NewLine +
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