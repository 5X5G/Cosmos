﻿using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Schema;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Core.PCInformation;
using Cosmos.Core.Plugs.CPUInformationPlugs;
using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.Plugs;

using XSharp.Compiler;

namespace Cosmos.Core.Plugs
{
    [Plug(Target = typeof(ProcessorInformation))]
    public unsafe class ProcessorInformationImpl
    {

        /// <summary>
        /// Use the rdtsc instruction to read the current time stamp counter
        /// In edx will be stored the highest part of the rtdsc
        /// In eax the lowest part of the rtdsc
        /// </summary>
        /// <param name="edx">Lowest part of rtdsc</param>
        /// <param name="eax">Highest part of rtdsc</param>
        [PlugMethod(Assembler = typeof(GetCurrentTimeStampCounter))]
        public static void GetCurrentTimeStampCounter(uint* edx, uint* eax) { }

        /// <summary>
        /// This function queries cpuid to get the registers involved.
        /// If a value is not used it will contain garbage.
        /// Requires that none of the arguments are null. THIS IS PROGRAMMER RESPONSABILITY
        /// </summary>
        /// call example <c>CPUID(0, &eax, &ebx, &ecx, &edx);</c> where eax, ebx, and edx are UINT
        /// <param name="eaxOperation">Number of the operation that cpuid will do.</param>
        /// <param name="eax">returned eax register (not null)</param>
        /// <param name="ebx">returned ebx register (not null)</param>
        /// <param name="ecx">returned ecx register (not null)</param>
        /// <param name="edx">returned edx register (not null)</param>
        [PlugMethod(Assembler= typeof(CPUID))]
        public static void CPUID(uint eaxOperation, uint* eax, uint* ebx, uint* ecx, uint* edx) { }

        [PlugMethod(Assembler = typeof(CanReadCPUID))]
        public static int CanReadCPUID() { return 0; }
    }
} 