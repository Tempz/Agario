/*
	Copyright (C) 2015 Tempz@users.noreply.github.com

	This file is part of https://github.com/Tempz/Agario

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;

namespace Agario.Http
{
    internal sealed class Packet
    {
        public byte[] Buffer;
        public int Index;

        public Packet(byte[] buffer, int index = 0)
        {
            Buffer = buffer;
            Index = index;
        }

        public byte ReadByte() => ReadByte(Buffer, ref Index);

        public ushort ReadUShort() => ReadUShort(Buffer, ref Index);

        public short ReadShort() => ReadShort(Buffer, ref Index);

        public uint ReadUInt() => ReadUInt(Buffer, ref Index);

        public int ReadInt() => ReadInt(Buffer, ref Index);

        public float ReadFloat() => ReadFloat(Buffer, ref Index);

        public double ReadDouble() => ReadDouble(Buffer, ref Index);

        private static byte[] ReadBytes(byte[] buffer, int length, ref int index)
        {
            if(index > buffer.Length - length) return new byte[length];

            byte[] b = new byte[length];
            Array.Copy(buffer, index, b, 0, length);
            if(length > 1 && !BitConverter.IsLittleEndian)
                Array.Reverse(b);
            index += length;
            return b;
        }

        public static byte ReadByte(byte[] buffer, ref int index) => ReadBytes(buffer, 1, ref index)[0];

        public static ushort ReadUShort(byte[] buffer, ref int index) => BitConverter.ToUInt16(ReadBytes(buffer, 2, ref index), 0);

        public static short ReadShort(byte[] buffer, ref int index) => BitConverter.ToInt16(ReadBytes(buffer, 2, ref index), 0);

        public static uint ReadUInt(byte[] buffer, ref int index) => BitConverter.ToUInt32(ReadBytes(buffer, 4, ref index), 0);

        public static int ReadInt(byte[] buffer, ref int index) => BitConverter.ToInt32(ReadBytes(buffer, 4, ref index), 0);

        public static float ReadFloat(byte[] buffer, ref int index) => BitConverter.ToSingle(ReadBytes(buffer, 4, ref index), 0);

        public static double ReadDouble(byte[] buffer, ref int index) => BitConverter.ToDouble(ReadBytes(buffer, 8, ref index), 0);
    }
}
