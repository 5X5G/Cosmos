﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cosmos.System.PCInfo
{
    public class Processor
    {
        /// <summary>
        /// Manufacturer of the procesor (on intel: genuine intel)
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// Processor family number
        /// </summary>
        public int Family{ get; set; }
        /// <summary>
        /// Flags of the processor (sse, fpu and so on)
        /// </summary>
        public List<int> Flags { get; set; }
        /// <summary>
        /// Stepping of the processor
        /// </summary>
        public int Stepping { get; set; }
        /// <summary>
        /// Model number
        /// </summary>
        public int ModelNumber { get; set; }

        public Processor()
        {
            Cosmos.HAL.PCInformation.Processor processor = new HAL.PCInformation.Processor();
            this.Flags = processor.Flags;
            this.Manufacturer = processor.Manufacturer;
            this.Family = processor.Family;
            this.ModelNumber = processor.ModelNumber;
            this.Stepping = processor.Stepping;
        }

    }
}
