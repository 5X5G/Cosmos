﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Common.Extensions;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.CPUInfo
{
    public unsafe class SMBIOS
    {
        public static CPUInfo.EntryPointTable BeginParseSMBIOS()
        {
            byte* memPtr = SMBIOSHandler.SearchEntryPointTable();

            CPUInfo.EntryPointTable entry =  SMBIOSHandler.ParseEntryTable(memPtr);

            //entry.GetTableAddress();
            BIOSInfo bios = SMBIOSHandler.ParseStructures(entry);
            return entry;
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

    }

    //This class contains the parser for the entry point table.
    //TODO: do checks for versions.
    unsafe class SMBIOSHandler
    {
        public static EntryPointTable ParseEntryTable(byte* memPtr)
        {
            EntryPointTable entryPointTable = new EntryPointTable();

            //Save the begginning address for future use
            entryPointTable.EntryPointAddr = memPtr;

            //Parse the _SM_
            for (int i = 0; i < 4; i++)
            {
                entryPointTable.EntryPointString[i] = (char)memPtr[i];
            }


            entryPointTable.CheckSum = memPtr[4];
            entryPointTable.Length = memPtr[5];
            entryPointTable.MajorVersion = memPtr[6];
            entryPointTable.MinorVersion = memPtr[7];

            //Create the array of byte to cast to ushort (to cast the maxStructureSize)
            byte[] tmpArray = new byte[2];
            tmpArray[0] = memPtr[8];
            tmpArray[1] = memPtr[9];
            entryPointTable.MaxStructureSize = BitConverter.ToUInt16(tmpArray, 0);

            //Continue parsing the table...
            entryPointTable.EntryPointRevision = memPtr[10];
            for (int i = 0; i < 5; i++)
            {
                entryPointTable.FormattedArea[i] = (char)memPtr[i + 11];
            }
            for (int i = 0; i < 5; i++)
            {
                entryPointTable.EntryPointString2[i] = (char) memPtr[i + 16];
            }

            entryPointTable.CheckSum2 = memPtr[21];
            tmpArray = new byte[2];
            tmpArray[0] = memPtr[22];
            tmpArray[1] = memPtr[23];
            entryPointTable.TableLength = ByteConverter.ToUInt16(tmpArray, 0);

            //Get the pointer to the main table
            // 4 bytes = 32 bits = address
            tmpArray = new byte[4];
            tmpArray[0] = memPtr[24];
            tmpArray[1] = memPtr[25];
            tmpArray[2] = memPtr[26];
            tmpArray[3] = memPtr[27];
            entryPointTable.TableAddress = (uint*)BitConverter.ToUInt32(tmpArray, 0);

            //Get the number of structures
            tmpArray = new byte[2];
            tmpArray[0] = memPtr[28];
            tmpArray[1] = memPtr[29];
            entryPointTable.NumberOfStructures = BitConverter.ToUInt16(tmpArray, 0);

            entryPointTable.BCDRevision = memPtr[30];

            return entryPointTable;
        }

        public static BIOSInfo ParseStructures(EntryPointTable entryPointTable)
        {
            BIOSInfo biosInfo = new BIOSInfo(entryPointTable, entryPointTable.GetTableAddress());
            byte* newMem = biosInfo.Parse();
            Debugger.DoSend("New table: " + newMem[0]);
            //Continue parsing
            return biosInfo;
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
