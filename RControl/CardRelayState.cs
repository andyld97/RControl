using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RControl
{
    public class CardRelayState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardRelayState"/> class.
        /// </summary>
        /// <param name="numberOfPorts">The number of ports available on the board.</param>
        /// <param name="cardAddress">The card address.</param>
        public CardRelayState(int numberOfPorts, int cardAddress)
        {
            this.CardState = new bool[numberOfPorts];
            this.CardAddress = cardAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CardRelayState"/> class.
        /// </summary>
        /// <param name="numberOfPorts">The number of ports.</param>
        /// <param name="cardAddress">The card address.</param>
        /// <param name="p1">Initial value vor port 1</param>
        /// <param name="p2">Initial value vor port 2</param>
        /// <param name="p3">Initial value vor port 3</param>
        /// <param name="p4">Initial value vor port 4</param>
        /// <param name="p5">Initial value vor port 5</param>
        /// <param name="p6">Initial value vor port 6</param>
        /// <param name="p7">Initial value vor port 7</param>
        /// <param name="p8">Initial value vor port 8</param>
        public CardRelayState(int numberOfPorts, int cardAddress, int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8)
            : this(numberOfPorts, cardAddress)
        {
            this.CardState[0] = Convert.ToBoolean(p1);
            this.CardState[1] = Convert.ToBoolean(p2);
            this.CardState[2] = Convert.ToBoolean(p3);
            this.CardState[3] = Convert.ToBoolean(p4);
            this.CardState[4] = Convert.ToBoolean(p5);
            this.CardState[5] = Convert.ToBoolean(p6);
            this.CardState[6] = Convert.ToBoolean(p7);
            this.CardState[7] = Convert.ToBoolean(p8);
        }

        /// <summary>
        /// Gets the state of the card.
        /// </summary>
        /// <value>
        /// The state of the card, rebresented as bool array
        /// </value>
        public bool[] CardState { get; private set; }

        /// <summary>
        /// Gets the card address.
        /// </summary>
        /// <value>
        /// The card address.
        /// </value>
        public int CardAddress { get; private set; }

        /// <summary>
        /// Set the card state one byte.
        /// </summary>
        /// <param name="state">The state.</param>
        public void FromByte(byte state)
        {
            var x = new BitArray(new[] { state });

            var idx = 0;
            for (var i = 0; i < x.Length; i++)
            {
                this.CardState[idx] = x[i];
                idx++;
            }
        }

        public void FromByte(bool[] state)
        {
            this.CardState = (bool[])state.Clone();
        }

        /// <summary>
        /// Get the card state as byte
        /// </summary>
        /// <returns>byte array representing the card state</returns>
        public byte[] ToByteArray()
        {
            var res = new BitArray(this.CardState.ToArray());
            var bytes = new byte[this.CardState.Length / 8];
            res.CopyTo(bytes, 0);
            return bytes;
        }

        public override string ToString()
        {
            return string.Format("[byte:{0}] mask:{1}", this.ToByteArray()[0].ToString(), Convert.ToString(this.ToByteArray()[0], 2));
        }
    }
}
