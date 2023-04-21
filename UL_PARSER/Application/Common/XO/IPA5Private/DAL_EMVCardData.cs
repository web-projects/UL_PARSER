using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UL_PARSER.Common.XO.IPA5Private
{
    /// <summary>
    /// These values are returned by the device.
    /// </summary>
    public class DAL_EMVCardData
    {
        /// <summary>
        /// Tag 57
        /// </summary>
        public string Track2EquivalentData { get; set; }

        public bool OfflineDeclined { get; set; }

        #region --- SRED AUTHENTICATION ---

        /// <summary>
        /// Tag DFDF11
        /// </summary>
        public string SREDKSN { get; set; }

        /// <summary>
        /// Tag DFDF10
        /// </summary>
        public string SREDEncryptedData { get; set; }

        /// <summary>
        /// Tag DFDF12
        /// </summary>
        public string SREDInputVector { get; set; }

        /// <summary>
        /// Tag DFDB0F
        /// </summary>
        public string SREDEncryptionStatus { get; set; }

        #endregion --- SRED AUTHENTICATION ---
        /// <summary>
        /// These tags will be used for EMV specific transactions instead
        /// </summary>
        public Dictionary<uint, byte[]> CapturedTagData { get; set; } = new Dictionary<uint, byte[]>();

        public DAL_EMVCardData AddTagData(uint tagValue, byte[] tagData, bool overwrite = true)
        {
            bool exist = CapturedTagData.TryAdd(tagValue, tagData);
            if (overwrite && !exist)
            {
                CapturedTagData[tagValue] = tagData;
            }

            return this;
        }

        public int? GetTagDataAsInt(uint input)
        {
            string tagData = GetTagData(input);
            if (tagData != null && int.TryParse(tagData, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int result))
            {
                return result;
            }

            return null;
        }

        public void CopyTag(DAL_EMVCardData source, params uint[] tagToCopy)
        {
            foreach (var tag in tagToCopy)
            {
                if (source.GetTagDataRaw(tag) is byte[] tagData)
                {
                    AddTagData(tag, tagData, true);
                }
            }
        }

        public byte[] GetTagDataRaw(uint input)
        {
            if (CapturedTagData.TryGetValue(input, out byte[] result))
            {
                return result;
            }
            return null;
        }

        public string GetTagData(uint input, bool isHexString = true)
        {
            if (CapturedTagData.TryGetValue(input, out byte[] result))
            {
                return isHexString ? BitConverter.ToString(result).Replace("-", "") : Encoding.UTF8.GetString(result);
            }
            return null;
        }
    }
}
