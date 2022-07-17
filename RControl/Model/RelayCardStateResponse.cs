namespace RControl.Model
{
    public class RelayCardStateResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCardStateResponse"/> class.
        /// </summary>
        public RelayCardStateResponse()
        {
            CardResponseFrame = new Conrad8RelayCardResponseFrame();
        }

        /// <summary>
        /// Gets or sets the card response frame.
        /// </summary>
        /// <value>
        /// The card response frame.
        /// </value>
        public Conrad8RelayCardResponseFrame CardResponseFrame { get; set; }

        /// <summary>
        /// Gets or sets the state of the relay.
        /// </summary>
        /// <value>
        /// The state of the relay.
        /// </value>
        public CardRelayState RelayState { get; set; }
    }
}
