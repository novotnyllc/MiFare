using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
#pragma warning disable 169
    

namespace MiFare.Win32
{
    class SafeNativeMethods
    {
       

        // *********************************************************************************************************
        // Function Name: SCardEstablishContext
        // In Parameter : dwScope - Scope of the resource manager context.
        //                pvReserved1 - Reserved for future use and must be NULL
        //                pvReserved2 - Reserved for future use and must be NULL.
        // Out Parameter: phContext - A handle to the established resource manager context
        // Description  : Establishes context to the reader
        //*********************************************************************************************************
        [DllImport("WinScard.dll", SetLastError = true)]
        public static extern int SCardEstablishContext(uint dwScope,
        IntPtr notUsed1,
        IntPtr notUsed2,
        out IntPtr phContext);


        // *********************************************************************************************************
        // Function Name: SCardReleaseContext
        // In Parameter : phContext - A handle to the established resource manager context              
        // Out Parameter: -------
        // Description  :Releases context from the reader
        //*********************************************************************************************************
        [DllImport("WinScard.dll", SetLastError = true)]
        public static extern int SCardReleaseContext(IntPtr phContext);


        // *********************************************************************************************************
        // Function Name: SCardConnect
        // In Parameter : hContext - A handle that identifies the resource manager context.
        //                cReaderName  - The name of the reader that contains the target card.
        //                dwShareMode - A flag that indicates whether other applications may form connections to the card.
        //                dwPrefProtocol - A bitmask of acceptable protocols for the connection.  
        // Out Parameter: ActiveProtocol - A flag that indicates the established active protocol.
        //                hCard - A handle that identifies the connection to the smart card in the designated reader. 
        // Description  : Connect to card on reader
        //*********************************************************************************************************
        [DllImport("WinScard.dll", SetLastError = true)]
        public static extern int SCardConnect(IntPtr hContext,
        string cReaderName,
        uint dwShareMode,
        uint dwPrefProtocol,
        out IntPtr hCard,
        out int ActiveProtocol);


        // *********************************************************************************************************
        // Function Name: SCardDisconnect
        // In Parameter : hCard - Reference value obtained from a previous call to SCardConnect.
        //                Disposition - Action to take on the card in the connected reader on close.  
        // Out(Parameter)
        // Description  : Disconnect card from reader
        //*********************************************************************************************************
        [DllImport("WinScard.dll", SetLastError = true)]
        public static extern int SCardDisconnect(IntPtr hCard, int Disposition);


        //    *********************************************************************************************************
        // Function Name: SCardListReaders
        // In Parameter : hContext - A handle to the established resource manager context
        //                mszReaders - Multi-string that lists the card readers with in the supplied readers groups
        //                pcchReaders - length of the readerlist buffer in characters
        // Out Parameter: mzGroup - Names of the Reader groups defined to the System
        //                pcchReaders - length of the readerlist buffer in characters
        // Description  : List of all readers connected to system 
        //*********************************************************************************************************
        [DllImport("WinScard.dll", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
        public static extern int SCardListReaders(
          IntPtr hContext,
          byte[] mszGroups,
          byte[] mszReaders,
          ref uint pcchReaders
          );


        // *********************************************************************************************************
        // Function Name: SCardState
        // In Parameter : hCard - Reference value obtained from a previous call to SCardConnect.
        // Out Parameter: state - Current state of smart card in  the reader
        //                protocol - Current Protocol
        //                ATR - 32 bytes buffer that receives the ATR string
        //                ATRLen - Supplies the length of ATR buffer
        // Description  : Current state of the smart card in the reader
        //*********************************************************************************************************
        [DllImport("WinScard.dll", SetLastError = true)]
        public static extern int SCardState(IntPtr hCard, ref IntPtr state, ref IntPtr protocol, ref byte[] ATR, ref int ATRLen);


        // *********************************************************************************************************
        // Function Name: SCardTransmit
        // In Parameter : hCard - A reference value returned from the SCardConnect function.
        //                pioSendRequest - A pointer to the protocol header structure for the instruction.
        //                SendBuff- A pointer to the actual data to be written to the card.
        //                SendBuffLen - The length, in bytes, of the pbSendBuffer parameter. 
        //                pioRecvRequest - Pointer to the protocol header structure for the instruction ,Pointer to the protocol header structure for the instruction, 
        //                followed by a buffer in which to receive any returned protocol control information (PCI) specific to the protocol in use.
        //                RecvBuffLen - Supplies the length, in bytes, of the pbRecvBuffer parameter and receives the actual number of bytes received from the smart card.
        // Out Parameter: pioRecvRequest - Pointer to the protocol header structure for the instruction ,Pointer to the protocol header structure for the instruction, 
        //                followed by a buffer in which to receive any returned protocol control information (PCI) specific to the protocol in use.
        //                RecvBuff - Pointer to any data returned from the card.
        //                RecvBuffLen - Supplies the length, in bytes, of the pbRecvBuffer parameter and receives the actual number of bytes received from the smart card.
        // Description  : Transmit APDU to card 
        //*********************************************************************************************************
        [DllImport("WinScard.dll", SetLastError = true)]
        public static extern int SCardTransmit(IntPtr hCard, ref SCARD_IO_REQUEST pioSendRequest,
                                                            byte[] SendBuff,
                                                            int SendBuffLen,
                                                            ref SCARD_IO_REQUEST pioRecvRequest,
                                                            byte[] RecvBuff, ref int RecvBuffLen);


        // *********************************************************************************************************
        // Function Name: SCardGetStatusChange
        // In Parameter : hContext - A handle that identifies the resource manager context.
        //                value_TimeOut - The maximum amount of time, in milliseconds, to wait for an action.
        //                ReaderState -  An array of SCARD_READERSTATE structures that specify the readers to watch, and that receives the result.
        //                ReaderCount -  The number of elements in the rgReaderStates array.
        // Out Parameter: ReaderState - An array of SCARD_READERSTATE structures that specify the readers to watch, and that receives the result.
        // Description  : The current availability of the cards in a specific set of readers changes.
        //*********************************************************************************************************
        [DllImport("winscard.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int SCardGetStatusChange(IntPtr hContext,
        int value_Timeout,
        ref SCARD_READERSTATE ReaderState,
        uint ReaderCount);
    }

    
}
