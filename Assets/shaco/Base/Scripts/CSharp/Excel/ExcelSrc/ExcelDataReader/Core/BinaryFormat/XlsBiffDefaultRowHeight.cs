﻿using System;
using System.Collections.Generic;
using System.Text;

namespace shaco.ExcelDataReader.Core.BinaryFormat
{
    internal class XlsBiffDefaultRowHeight : XlsBiffRecord
    {
        public XlsBiffDefaultRowHeight(byte[] bytes, uint offset, int biffVersion)
            : base(bytes, offset)
        {
            if (biffVersion == 2)
            {
                RowHeight = ReadUInt16(0x0) & 0x7FFF;
            }
            else
            {
                var flags = (DefaultRowHeightFlags)ReadUInt16(0x0);
                RowHeight = (flags & DefaultRowHeightFlags.DyZero) == 0 ? ReadUInt16(0x2) : 0;
                
                // UnhiddenRowHeight => (Flags & DefaultRowHeightFlags.DyZero) != 0 ? ReadInt16(0x2) : 0;
            }
        }

        internal enum DefaultRowHeightFlags : ushort
        {
            Unsynced = 1,
            DyZero = 2,
            ExAsc = 4,
            ExDsc = 8
        }

        /// <summary>
        /// Gets the row height in twips
        /// </summary>
        public int RowHeight { get; }
    }
}