using Mapbox.VectorTile.Contants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Mapbox.VectorTile
{
    public class PbfReader
    {
        private byte[] _buffer;

        private ulong _length;

        private ulong _pos;

        public int Tag
        {
            get;
            private set;
        }

        public ulong Value
        {
            get;
            private set;
        }

        public WireTypes WireType
        {
            get;
            private set;
        }

        public PbfReader(byte[] tileBuffer)
        {
            _buffer = tileBuffer;
            _length = (ulong)_buffer.Length;
            WireType = WireTypes.UNDEFINED;
        }

        public long Varint()
        {
            int i = 0;
            long num = 0L;
            for (; i < 64; i += 7)
            {
                byte b = _buffer[_pos];
                num |= (long)(b & 0x7F) << i;
                _pos += 1uL;
                if ((b & 0x80) == 0)
                {
                    return num;
                }
            }
            throw new ArgumentException("Invalid varint");
        }

        public byte[] View()
        {
            if (Tag == 0)
            {
                throw new Exception("call next() before accessing field value");
            }
            if (WireType != WireTypes.BYTES)
            {
                throw new Exception("not of type string, bytes or message");
            }
            ulong num = (ulong)Varint();
            SkipBytes(num);
            byte[] array = new byte[num];
            Array.Copy(_buffer, (int)_pos - (int)num, array, 0, (int)num);
            return array;
        }

        public List<uint> GetPackedUnit32()
        {
            List<uint> list = new List<uint>(200);
            ulong num = (ulong)Varint();
            ulong num2 = _pos + num;
            while (_pos < num2)
            {
                list.Add((uint)Varint());
            }
            return list;
        }

        public List<int> GetPackedSInt32()
        {
            List<int> list = new List<int>(200);
            ulong num = (ulong)Varint();
            ulong num2 = _pos + num;
            while (_pos < num2)
            {
                list.Add(decodeZigZag32((int)Varint()));
            }
            return list;
        }

        public List<long> GetPackedSInt64()
        {
            List<long> list = new List<long>(200);
            ulong num = (ulong)Varint();
            ulong num2 = _pos + num;
            while (_pos < num2)
            {
                list.Add(decodeZigZag64(Varint()));
            }
            return list;
        }

        private int decodeZigZag32(int value)
        {
            return value >> 1 ^ -(value & 1);
        }

        private long decodeZigZag64(long value)
        {
            return value >> 1 ^ -(value & 1);
        }

        public double GetDouble()
        {
            byte[] array = new byte[8];
            Array.Copy(_buffer, (int)_pos, array, 0, 8);
            _pos += 8uL;
            return BitConverter.ToDouble(array, 0);
        }

        public float GetFloat()
        {
            byte[] array = new byte[4];
            Array.Copy(_buffer, (int)_pos, array, 0, 4);
            _pos += 4uL;
            return BitConverter.ToSingle(array, 0);
        }

        public string GetString(ulong length)
        {
            byte[] array = new byte[length];
            Array.Copy(_buffer, (int)_pos, array, 0, (int)length);
            _pos += length;
            return Encoding.UTF8.GetString(array, 0, array.Length);
        }

        public bool NextByte()
        {
            if (_pos >= _length)
            {
                return false;
            }
            Value = (ulong)Varint();
            Tag = (int)Value >> 3;
            if ((Tag == 0 || Tag >= 19000) && (Tag > 19999 || Tag <= 536870911))
            {
                throw new Exception("tag out of range");
            }
            WireType = (WireTypes)(Value & 7);
            return true;
        }

        public void SkipVarint()
        {
            Varint();
        }

        public void SkipBytes(ulong skip)
        {
            if (_pos + skip > _length)
            {
                string message = string.Format(NumberFormatInfo.InvariantInfo, "[SkipBytes()] skip:{0} pos:{1} len:{2}", skip, _pos, _length);
                throw new Exception(message);
            }
            _pos += skip;
        }

        public ulong Skip()
        {
            if (Tag == 0)
            {
                throw new Exception("call next() before calling skip()");
            }
            switch (WireType)
            {
                case WireTypes.VARINT:
                    SkipVarint();
                    break;
                case WireTypes.BYTES:
                    SkipBytes((ulong)Varint());
                    break;
                case WireTypes.FIXED32:
                    SkipBytes(4uL);
                    break;
                case WireTypes.FIXED64:
                    SkipBytes(8uL);
                    break;
                case WireTypes.UNDEFINED:
                    throw new Exception("undefined wire type");
                default:
                    throw new Exception("unknown wire type");
            }
            return _pos;
        }
    }
}
