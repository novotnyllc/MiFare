using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    /// <summary>
    ///     Class that handles the access conditions to a given sector of the card
    /// </summary>
    public class AccessConditions
    {
        /// <summary>
        ///     Version of the MAD supported by the card. The MAD version is written only in the trailer datablock of sector 0.
        ///     For all other sector, this value has no meaning
        /// </summary>
        public enum MADVersionEnum
        {
            NoMAD,
            Version1,
            Version2
        }

        /// <summary>
        ///     Access conditions for each data area. This array has always 3 elements
        /// </summary>
        public DataAreaAccessCondition[] DataAreas;

        public MADVersionEnum MADVersion;

        /// <summary>
        ///     True if the card supports multiple applications
        /// </summary>
        public bool MultiApplicationCard;

        /// <summary>
        ///     Access conditions for the trailer datablock
        /// </summary>
        public TrailerAccessCondition Trailer;

        public AccessConditions()
        {
            DataAreas = new DataAreaAccessCondition[3];
            DataAreas[0] = new DataAreaAccessCondition();
            DataAreas[1] = new DataAreaAccessCondition();
            DataAreas[2] = new DataAreaAccessCondition();

            Trailer = new TrailerAccessCondition();

            MADVersion = MADVersionEnum.NoMAD;
            MultiApplicationCard = false;
        }
    }
}