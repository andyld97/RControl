﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RControl
{
    /// <summary>
    /// Holds all contents of a card response frame
    /// </summary>
    public class Conrad8RelayCardResponseFrame
    {
        /// <summary>
        /// Gets or sets the response command.
        /// </summary>
        /// <value>
        /// The response command.
        /// </value>
        public Conrad8RelayCard.ResponseCommand ResponseCommand { get; set; }

        /// <summary>
        /// Gets or sets the address byte.
        /// </summary>
        /// <value>
        /// The address byte.
        /// </value>
        public byte AddressByte { get; set; }

        /// <summary>
        /// Gets or sets the data byte.
        /// </summary>
        /// <value>
        /// The data byte.
        /// </value>
        public byte DataByte { get; set; }

        /// <summary>
        /// Gets or sets the CRC byte.
        /// </summary>
        /// <value>
        /// The CRC byte.
        /// </value>
        public byte CrcByte { get; set; }

        public override string ToString()
        {
            return string.Format("[ADR:{0} DAT:{2}, CRC:{3}]", this.AddressByte, this.ResponseCommand, this.DataByte, this.CrcByte);
        }

    }
}
