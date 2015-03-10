using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace MiFare.Win32
{
    static class Constants
    {
        // Context Scope
        /// <summary>
        /// The context is a user context, and any
        /// database operations are performed within the
        /// domain of the user.
        /// </summary>
        public const uint SCARD_SCOPE_USER = 0;    

        /// <summary>
        /// The context is that of the current terminal,
        /// and any database operations are performed
        /// within the domain of that terminal.  (The
        /// calling application must have appropriate
        /// access permissions for any database actions.)
        /// </summary>
        public const uint SCARD_SCOPE_TERMINAL = 1;    

        /// <summary>
        /// The context is the system context, and any
        /// database operations are performed within the
        /// domain of the system.  (The calling
        /// application must have appropriate access
        /// permissions for any database actions.)
        /// </summary>
        public const uint SCARD_SCOPE_SYSTEM = 2;   
      


        public const int SCARD_STATE_UNAWARE = 0x0;

        //The application is unaware about the curent state, This value results in an immediate return
        //from state transition monitoring services. This is represented by all bits set to zero
        public const int SCARD_SHARE_SHARED = 2;

        // Application will share this card with other 
        // applications.

        //   Disposition
        public const int SCARD_UNPOWER_CARD = 2; // Power down the card on close

        //   PROTOCOL
        public const int SCARD_PROTOCOL_T0 = 0x1;                  // T=0 is the active protocol.
        public const int SCARD_PROTOCOL_T1 = 0x2;                  // T=1 is the active protocol.
        public const int SCARD_PROTOCOL_UNDEFINED = 0x0;

      

        //Card Type
        public const int card_Type_Mifare_1K = 1;
        public const int card_Type_Mifare_4K = 2;

    }
}
