﻿using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.SMBIOS
{

    //This class contains the parser for the entry point table.
    //TODO: do checks for versions.
    public unsafe class SMBIOS
    {
        public static SMBIOSStructure BeginParseSMBIOS()
        {
            byte* memPtr = SMBIOS.SearchEntryPointTable();

            EntryPointTable entry =  new EntryPointTable();
            //We dont return an address since we need to use a pointer that
            //its inside the table
            entry.Parse(memPtr);

            //entry.GetTableAddress();
            SMBIOSStructure smbiosStructure = SMBIOS.ParseStructures(entry);
            smbiosStructure.EntryPointTable = entry;
            return smbiosStructure;
        }

        /// <summary>
        /// Parses a string of type smbios and stores the result in the variable "variable"
        /// This function stops when found a nul byte (i.e, \0)
        /// Note that the contents of variable will be overwritten.
        /// </summary>
        /// <param name="beginningAddress">Address in which we start searching</param>
        /// <param name="variable">Variable in which we will store the result</param>
        /// <returns>Offset of the search (i.e, the number of position searched)</returns>
        public static byte* ParseString(byte* beginningAddress, out string variable)
        {
            variable = "";
            var i = 0;
            while (beginningAddress[i] != '\0')
            {
                variable = variable + (char)beginningAddress[i];
                i++;
            }
            //We need to add one to skip the \0
            return beginningAddress + i + 1;
        }

        public static SMBIOSStructure ParseStructures(EntryPointTable entryPointTable)
        {
            SMBIOSStructure smbiosStructure = new SMBIOSStructure();
            List<CPUInfo> cpuList = new List<CPUInfo>();
            byte* currentAddress = entryPointTable.GetTableAddress();
            for (int i = 0; i < entryPointTable.NumberOfStructures; i++)
            {
                //We need to compare the type (which will be always the 0 fo current address)
                switch (Convert.ToUInt32((currentAddress)[0]))
                {
                    case 0:
                        if (smbiosStructure.BiosInfo == null)
                        {
                            smbiosStructure.BiosInfo = new BIOSInfo(entryPointTable, currentAddress);
                            currentAddress = smbiosStructure.BiosInfo.Parse();
                            //DebugSMBIOS.DebugBIOSInfo(smbiosStructure.BiosInfo);
                        }
                        else
                        {
                            //If we fail skipping the table
                            currentAddress = currentAddress + 1;
                        }
                        break;
                    case 4:
                        CPUInfo cpuInfo = new CPUInfo(entryPointTable, currentAddress);
                        currentAddress = cpuInfo.Parse();
                        smbiosStructure.CpuInfoList.Add(cpuInfo);
                        //DebugSMBIOS.DebugCPUInfo(cpuInfo);
                        break;
                    default:
                        //In [1] we have the length of the formatted section.
                        currentAddress = SkipTable(currentAddress[1], currentAddress);
                        break;
                }
            }

            return smbiosStructure;
        }

        /// <summary>
        /// Skip a table with length x
        ///We need the length since the table can contain double nulls inside the formatted section
        /// </summary>
        /// <param name="length">Length of the table to skip</param>
        /// <param name="beginningAddress">Address to search</param>
        /// <returns></returns>
        public static byte* SkipTable(int length, byte* beginningAddress)
        {
            int i;
            //Skip the formatted section
            for (i = 0; i < length; i++) ;

            //Skip the unformatted section (bunch of strings)
            for (;;)
            {
                //If we found the double null we finished the table
                if (beginningAddress[i] == 0 && beginningAddress[i + 1] == 0)
                {
                    //The pointer will be situated under the first null
                    //We sum 2 to skip this
                    return beginningAddress + i + 2;
                }
                //We need to increment here.
                //If we increment before checking for double null it doesnt work
                //for tables without strings
                i++;
            }

        }

        /// <summary>
        /// This method searches the memory region so as to locate the
        /// "Entry Point Table"
        /// </summary>
        /// <returns>Pointer to the beggining of the table</returns>
        public static byte* SearchEntryPointTable()
        {
            string str = "";
            byte* memPtr;
            int length, checksum;
            for (memPtr = (byte*) 0xf0000;
                memPtr < (byte*)0x100000;
                memPtr += 16)
            {
                if (memPtr[0] == '_' && memPtr[1] == 'S' && memPtr[2] == 'M' && memPtr[3] == '_')
                {

                    //TODO: do checksum
                    /*
                    length = memPtr[5];
                    checksum = 0;
                    for (int i = 0; i < length; i++)
                    {
                        checksum += memPtr[i];
                        str += (char)memPtr[i];
                    }
                    if (checksum == 0)
                    */
                    return memPtr;
                }
            }
            return null;

        }
    }
}
