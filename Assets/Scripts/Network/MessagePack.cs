using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Elpis.Message
{
    public static class MessagePack
    {
        public static bool Pack(MemoryStream _ms, Dictionary<object, object> _dict, bool _legacy = false)
        {
            if (_ms == null)
                return false;

            return PackObject(_ms, _dict, _legacy);
        }

        public static bool Pack(MemoryStream _ms, List<object> _list, bool _legacy = false)
        {
            if (_ms == null)
                return false;

            return PackObject(_ms, _list, _legacy);
        }

        public static bool Unpack(MemoryStream _ms, out MessagePackObject _obj, bool _legacy = false)
        {
            _obj = null;

            if (_ms == null)
                return false;

            return UnpackObject(_ms, out _obj, _legacy);
        }

        #region Pack
        private static bool PackList(MemoryStream _ms, List<object> _list, bool _legacy)
        {
            if (_list == null)
                return false;

            if (_list.Count < 16)
            {
                _ms.WriteByte((byte)(0x90 | _list.Count));
            }
            else if (_list.Count <= ushort.MaxValue)
            {
                _ms.WriteByte(0xdc);
                _ms.Write(BitConverter.GetBytes((ushort)_list.Count), 0, 2, BitConverter.IsLittleEndian);
            }
            else
            {
                _ms.WriteByte(0xdd);
                _ms.Write(BitConverter.GetBytes(_list.Count), 0, 4, BitConverter.IsLittleEndian);
            }

            for (int i = 0; i < _list.Count; i++)
            {
                if (!PackObject(_ms, _list[i], _legacy))
                    return false;
            }

            return true;
        }

        private static bool PackMap(MemoryStream _ms, Dictionary<object, object> _dict, bool _legacy)
        {
            if (_dict == null)
                return false;

            if (_dict.Count < 16)
            {
                _ms.WriteByte((byte)(0x80 | _dict.Count));
            }
            else if (_dict.Count <= ushort.MaxValue)
            {
                _ms.WriteByte(0xde);
                _ms.Write(BitConverter.GetBytes((ushort)_dict.Count), 0, 2, BitConverter.IsLittleEndian);
            }
            else
            {
                _ms.WriteByte(0xdf);
                _ms.Write(BitConverter.GetBytes((uint)_dict.Count), 0, 4, BitConverter.IsLittleEndian);
            }

            foreach (KeyValuePair<object, object> kv in _dict)
            {
                if (!(PackObject(_ms, kv.Key, _legacy) && PackObject(_ms, kv.Value, _legacy)))
                    return false;
            }

            return true;
        }

        private static bool PackExtension(MemoryStream _ms, IMsgPackExtension _ext)
        {
            if (_ext.GetExtType() < 0)
                return false;

            byte type = (byte)_ext.GetExtType();
            byte[] data = _ext.GetRawData();

            if (data.Length == 1)
            {
                _ms.WriteByte(0xd4);
                _ms.WriteByte(type);
                _ms.WriteByte(data[0]);
            }
            else if (data.Length == 2)
            {
                _ms.WriteByte(0xd5);
                _ms.WriteByte(type);
                _ms.Write(data, 0, 2);
            }
            else if (data.Length == 4)
            {
                _ms.WriteByte(0xd6);
                _ms.WriteByte(type);
                _ms.Write(data, 0, 4);
            }
            else if (data.Length == 8)
            {
                _ms.WriteByte(0xd7);
                _ms.WriteByte(type);
                _ms.Write(data, 0, 8);
            }
            else if (data.Length == 16)
            {
                _ms.WriteByte(0xd8);
                _ms.WriteByte(type);
                _ms.Write(data, 0, 16);
            }
            else if (data.Length <= byte.MaxValue)
            {
                _ms.WriteByte(0xc7);
                _ms.WriteByte((byte)data.Length);
                _ms.WriteByte(type);
                _ms.Write(data, 0, data.Length);
            }
            else if (data.Length <= ushort.MaxValue)
            {
                _ms.WriteByte(0xc8);
                _ms.Write(BitConverter.GetBytes((ushort)data.Length), 0, 2, BitConverter.IsLittleEndian);
                _ms.WriteByte(type);
                _ms.Write(data, 0, data.Length);
            }
            else
            {
                _ms.WriteByte(0xc9);
                _ms.Write(BitConverter.GetBytes((uint)data.Length), 0, 4, BitConverter.IsLittleEndian);
                _ms.WriteByte(type);
                _ms.Write(data, 0, data.Length);
            }

            return true;
        }

        private static bool PackStringBytes(MemoryStream _ms, byte[] _str, bool _legacy)
        {
            if (_str.Length < 32)
            {
                _ms.WriteByte((byte)(0xa0 | _str.Length));
                _ms.Write(_str, 0, _str.Length);
            }
            else if (_str.Length < byte.MaxValue && !_legacy)
            {
                _ms.WriteByte(0xd9);
                _ms.WriteByte((byte)_str.Length);
                _ms.Write(_str, 0, _str.Length);
            }
            else if (_str.Length < ushort.MaxValue)
            {
                _ms.WriteByte(0xda);
                _ms.Write(BitConverter.GetBytes((ushort)_str.Length), 0, 2, BitConverter.IsLittleEndian);
                _ms.Write(_str, 0, _str.Length);
            }
            else
            {
                _ms.WriteByte(0xdb);
                _ms.Write(BitConverter.GetBytes((uint)_str.Length), 0, 4, BitConverter.IsLittleEndian);
                _ms.Write(_str, 0, _str.Length);
            }

            return true;
        }

        private static bool PackByteArray(MemoryStream _ms, byte[] _data)
        {
            if (_data.Length < byte.MaxValue)
            {
                _ms.WriteByte(0xc4);
                _ms.WriteByte((byte)_data.Length);
                _ms.Write(_data, 0, _data.Length);
            }
            else if (_data.Length < ushort.MaxValue)
            {
                _ms.WriteByte(0xc5);
                _ms.Write(BitConverter.GetBytes((ushort)_data.Length), 0, 2, BitConverter.IsLittleEndian);
                _ms.Write(_data, 0, _data.Length);
            }
            else
            {
                _ms.WriteByte(0xc6);
                _ms.Write(BitConverter.GetBytes((uint)_data.Length), 0, 4, BitConverter.IsLittleEndian);
                _ms.Write(_data, 0, _data.Length);
            }

            return true;
        }

        private static bool PackObject(MemoryStream _ms, object _obj, bool _legacy)
        {
            Type type = (_obj == null ? null : _obj.GetType());
            TypeCode code = (type == null ? TypeCode.Empty : Type.GetTypeCode(type));

            switch (code)
            {
                case TypeCode.Empty:
                    {
                        _ms.WriteByte(0xc0);
                        return true;
                    }
                case TypeCode.Boolean:
                    {
                        _ms.WriteByte((byte)((bool)_obj ? 0xc3 : 0xc2));
                        return true;
                    }
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    {
                        long value = Convert.ToInt64(_obj);

                        if (value >= 0 && value <= 127)
                        {
                            // 0x00 - 0x7f
                            byte b = (byte)value;
                            _ms.WriteByte(b);
                        }
                        else if (value >= -32 && value < 0)
                        {
                            // 0xe0 - 0xff
                            sbyte sb = (sbyte)value;
                            _ms.WriteByte((byte)sb);
                        }
                        else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                        {
                            // 0xd0
                            sbyte sb = (sbyte)value;
                            _ms.WriteByte(0xd0);
                            _ms.WriteByte((byte)sb);
                        }
                        else if (value >= short.MinValue && value <= short.MaxValue)
                        {
                            // 0xd1
                            short s = (short)value;
                            _ms.WriteByte(0xd1);
                            _ms.Write(BitConverter.GetBytes(s), 0, 2, BitConverter.IsLittleEndian);
                        }
                        else if (value >= int.MinValue && value <= int.MaxValue)
                        {
                            // 0xd2
                            int i = (int)value;
                            _ms.WriteByte(0xd2);
                            _ms.Write(BitConverter.GetBytes(i), 0, 4, BitConverter.IsLittleEndian);
                        }
                        else
                        {
                            // 0xd3
                            _ms.WriteByte(0xd3);
                            _ms.Write(BitConverter.GetBytes(value), 0, 8, BitConverter.IsLittleEndian);
                        }

                        return true;
                    }
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    {
                        ulong value = Convert.ToUInt64(_obj);

                        if (value >= 0 && value <= 127)
                        {
                            // 0x00 - 0x7f
                            byte b = (byte)value;
                            _ms.WriteByte(b);
                        }
                        else if (value >= byte.MinValue && value <= byte.MaxValue)
                        {
                            // 0xcc
                            byte sb = (byte)value;
                            _ms.WriteByte(0xcc);
                            _ms.WriteByte(sb);
                        }
                        else if (value >= ushort.MinValue && value <= ushort.MaxValue)
                        {
                            // 0xcd
                            ushort s = (ushort)value;
                            _ms.WriteByte(0xcd);
                            _ms.Write(BitConverter.GetBytes(s), 0, 2, BitConverter.IsLittleEndian);
                        }
                        else if (value >= uint.MinValue && value <= uint.MaxValue)
                        {
                            // 0xce
                            int i = (int)value;
                            _ms.WriteByte(0xce);
                            _ms.Write(BitConverter.GetBytes(i), 0, 4, BitConverter.IsLittleEndian);
                        }
                        else
                        {
                            // 0xcf
                            _ms.WriteByte(0xcf);
                            _ms.Write(BitConverter.GetBytes(value), 0, 8, BitConverter.IsLittleEndian);
                        }

                        return true;
                    }
                case TypeCode.Single:
                    {
                        float f = (float)_obj;

                        _ms.WriteByte(0xca);
                        _ms.Write(BitConverter.GetBytes(f), 0, 4, BitConverter.IsLittleEndian);
                        return true;
                    }
                case TypeCode.Double:
                    {
                        double d = (double)_obj;

                        _ms.WriteByte(0xcb);
                        _ms.Write(BitConverter.GetBytes(d), 0, 8, BitConverter.IsLittleEndian);
                        return true;
                    }
                case TypeCode.String:
                    {
                        return PackStringBytes(_ms, Encoding.UTF8.GetBytes((string)_obj), _legacy);
                    }
                default:
                    {
                        if (type == typeof(byte[]))
                        {
                            byte[] data = (byte[])_obj;

                            if (_legacy)
                                return PackStringBytes(_ms, data, _legacy);

                            return PackByteArray(_ms, data);
                        }
                        else if (type == typeof(List<object>))
                        {
                            return PackList(_ms, (List<object>)_obj, _legacy);
                        }
                        else if (type == typeof(Dictionary<object, object>))
                        {
                            return PackMap(_ms, (Dictionary<object, object>)_obj, _legacy);
                        }
                        else if (type.IsAssignableFrom(typeof(IMsgPackExtension)) && !_legacy)
                        {
                            return PackExtension(_ms, (IMsgPackExtension)_obj);
                        }
                        break;
                    }
            }

            return false;
        }
        #endregion Pack

        #region Unpack
        private static bool UnpackObject(MemoryStream _ms, out MessagePackObject _obj, bool _legacy)
        {
            _obj = null;

            int format = _ms.ReadByte();

            // reach to end of stream
            if (format < 0)
                return false;

            long pos = _ms.Seek(0, SeekOrigin.Current);  // get real position in buffer
            byte[] buf = _ms.GetBuffer();

            switch (format)
            {
                case 0xc0:  // nil
                    {
                        _obj = new MessagePackObject();
                        return true;
                    }
                case 0xc2:  // boolean false
                    {
                        _obj = new MessagePackObject(false);
                        return true;
                    }
                case 0xc3:  // boolean true
                    {
                        _obj = new MessagePackObject(true);
                        return true;
                    }
                case 0xca:  // float
                    {
                        byte[] tmp = new byte[4];
                        _ms.Read(tmp, 0, 4, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToSingle(tmp, 0));
                        return true;
                    }
                case 0xcb:  // double
                    {
                        byte[] tmp = new byte[8];
                        _ms.Read(tmp, 0, 8, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToDouble(tmp, 0));
                        return true;
                    }
                case 0xc4:
                case 0xc5:
                case 0xc6:  // byte array
                    {
                        byte[] tmp = new byte[4];
                        long length = (1 << (format - 0xc4));

                        _ms.Read(tmp, 0, (int)length, BitConverter.IsLittleEndian);
                        pos += length;
                        length = BitConverter.ToUInt32(tmp, 0);

                        _obj = new MessagePackObject(buf, pos, length, _legacy);
                        _ms.Position += length;

                        return true;
                    }
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf:
                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:  // fixed str
                    {
                        int length = (format & 0x1f);

                        _obj = (_legacy ? new MessagePackObject(buf, (int)pos, length, _legacy) :
                                        new MessagePackObject(Encoding.UTF8.GetString(buf, (int)pos, length)));

                        _ms.Position += length;

                        return true;
                    }
                case 0xd9:
                case 0xda:
                case 0xdb:  // str
                    {
                        byte[] tmp = new byte[4];
                        int length = (1 << (format - 0xd9));

                        _ms.Read(tmp, 0, length, BitConverter.IsLittleEndian);
                        pos += length;
                        length = (int)BitConverter.ToUInt32(tmp, 0);

                        _obj = (_legacy ? new MessagePackObject(buf, (int)pos, length, _legacy) :
                                        new MessagePackObject(Encoding.UTF8.GetString(buf, (int)pos, length)));

                        _ms.Position += length;

                        return true;
                    }
                case 0xe0:
                case 0xe1:
                case 0xe2:
                case 0xe3:
                case 0xe4:
                case 0xe5:
                case 0xe6:
                case 0xe7:
                case 0xe8:
                case 0xe9:
                case 0xea:
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                case 0xf0:
                case 0xf1:
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                case 0xf6:
                case 0xf7:
                case 0xf8:
                case 0xf9:
                case 0xfa:
                case 0xfb:
                case 0xfc:
                case 0xfd:
                case 0xfe:
                case 0xff:  // negative fixed int
                    {
                        _obj = new MessagePackObject((sbyte)format);
                        return true;
                    }
                case 0xcc:  // uint8
                    {
                        _obj = new MessagePackObject(buf[pos]);
                        _ms.Position++;
                        return true;
                    }
                case 0xcd:  // uint16
                    {
                        byte[] tmp = new byte[2];
                        _ms.Read(tmp, 0, 2, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToUInt16(tmp, 0));
                        return true;
                    }
                case 0xce:  // uint32
                    {
                        byte[] tmp = new byte[4];
                        _ms.Read(tmp, 0, 4, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToUInt32(tmp, 0));
                        return true;
                    }
                case 0xcf:  // uint64
                    {
                        byte[] tmp = new byte[8];
                        _ms.Read(tmp, 0, 8, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToUInt64(tmp, 0));
                        return true;
                    }
                case 0xd0:  // int8
                    {
                        _obj = new MessagePackObject((sbyte)buf[pos]);
                        _ms.Position++;
                        return true;
                    }
                case 0xd1:  // int16
                    {
                        byte[] tmp = new byte[2];
                        _ms.Read(tmp, 0, 2, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToInt16(tmp, 0));
                        return true;
                    }
                case 0xd2:  // int32
                    {
                        byte[] tmp = new byte[4];
                        _ms.Read(tmp, 0, 4, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToInt32(tmp, 0));
                        return true;
                    }
                case 0xd3:  // int64
                    {
                        byte[] tmp = new byte[8];
                        _ms.Read(tmp, 0, 8, BitConverter.IsLittleEndian);

                        _obj = new MessagePackObject(BitConverter.ToInt64(tmp, 0));
                        return true;
                    }
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97:
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f:  // fixed array
                    {
                        int length = (format & 0x0f);
                        List<MessagePackObject> list = new List<MessagePackObject>(length);

                        for (int i = 0; i < length; i++)
                        {
                            MessagePackObject element;

                            if (UnpackObject(_ms, out element, _legacy))
                                list.Add(element);
                            else
                                return false;
                        }

                        _obj = new MessagePackObject(list);

                        return true;
                    }
                case 0xdc:  // array 16
                case 0xdd:  // array 32
                    {
                        uint length = (1U << (format - (0xdc - 1)));
                        byte[] tmp = new byte[4];
                        _ms.Read(tmp, 0, (int)length, BitConverter.IsLittleEndian);

                        length = (length == 2 ? BitConverter.ToUInt16(tmp, 0) : BitConverter.ToUInt32(tmp, 0));
                        List<MessagePackObject> list = new List<MessagePackObject>((int)length);

                        for (uint i = 0; i < length; i++)
                        {
                            MessagePackObject element;

                            if (UnpackObject(_ms, out element, _legacy))
                                list.Add(element);
                            else
                                return false;
                        }

                        _obj = new MessagePackObject(list);

                        return true;
                    }
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87:
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f:  // fixed map
                    {
                        int count = (format & 0x0f);
                        Dictionary<MessagePackObject, MessagePackObject> dict = new Dictionary<MessagePackObject, MessagePackObject>(count);

                        for (int i = 0; i < count; i++)
                        {
                            MessagePackObject key, value;

                            if (UnpackObject(_ms, out key, _legacy) && UnpackObject(_ms, out value, _legacy))
                                dict[key] = value;
                            else
                                return false;
                        }

                        _obj = new MessagePackObject(dict);

                        return true;
                    }
                case 0xde:  // map16
                case 0xdf:  // map32
                    {
                        uint count = (1U << (format - (0xde - 1)));
                        byte[] tmp = new byte[count];
                        _ms.Read(tmp, 0, (int)count, BitConverter.IsLittleEndian);

                        count = (count == 2 ? BitConverter.ToUInt16(tmp, 0) : BitConverter.ToUInt32(tmp, 0));
                        Dictionary<MessagePackObject, MessagePackObject> dict = new Dictionary<MessagePackObject, MessagePackObject>((int)count);

                        for (uint i = 0; i < count; i++)
                        {
                            MessagePackObject key, value;

                            if (UnpackObject(_ms, out key, _legacy) && UnpackObject(_ms, out value, _legacy))
                                dict[key] = value;
                            else
                                return false;
                        }

                        _obj = new MessagePackObject(dict);

                        return true;
                    }
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7:  // fixext
                case 0xd8:
                    {
                        sbyte type = (sbyte)buf[pos];
                        int count = (1 << (format - 0xd4));
                        byte[] tmp = new byte[count];

                        _ms.Position++;
                        _ms.Read(tmp, 0, count);

                        _obj = new MessagePackExtObject(type, tmp);
                        return true;
                    }
                case 0xc7:
                case 0xc8:
                case 0xc9:  // ext
                    {
                        uint length = (1U << (format - 0xc7));
                        sbyte type = (sbyte)buf[pos + length];
                        byte[] tmp = new byte[length];

                        _ms.Read(tmp, 0, (int)length, BitConverter.IsLittleEndian);
                        _ms.Position++;
                        length = BitConverter.ToUInt32(tmp, 0);

                        tmp = new byte[length];
                        _ms.Read(tmp, 0, (int)length);

                        _obj = new MessagePackExtObject(type, tmp);

                        return true;
                    }
                case 0xc1:
                    {
                        // never used!
                        return false;
                    }
            }

            // positive fixint
            _obj = new MessagePackObject((byte)format);
            return true;
        }
        #endregion Unpack

        #region Tools
        private static void Write(this MemoryStream _ms, byte[] _data, int _offset, int _length, bool _reverse)
        {
            if (_reverse)
            {
                byte[] buf = _ms.GetBuffer();
                long pos = _ms.Seek(0, SeekOrigin.Current);

                _ms.Position += _length;

                // extend length
                if (_ms.Length < _ms.Position)
                {
                    _ms.SetLength(_ms.Length + _length);
                    buf = _ms.GetBuffer();
                }

                while (--_length >= 0)
                    buf[pos + _length] = _data[_offset++];
            }
            else
            {
                _ms.Write(_data, _offset, _length);
            }
        }

        private static void Read(this MemoryStream _ms, byte[] _dest, int _offset, int _length, bool _reverse)
        {
            if (_reverse)
            {
                byte[] buf = _ms.GetBuffer();
                long pos = _ms.Seek(0, SeekOrigin.Current);

                _ms.Position += _length;

                while (--_length >= 0)
                    _dest[_offset++] = buf[pos + _length];
            }
            else
            {
                _ms.Read(_dest, _offset, _length);
            }
        }

        public static bool TryGetValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out MessagePackObject _obj)
        {
            return _dict.TryGetValue(new MessagePackObject(_key), out _obj);
        }

        public static bool TryGetBooleanValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out bool _value, bool _fallback = false)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsBoolean(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetByteValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out byte _value, byte _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsByte(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetSByteValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out sbyte _value, sbyte _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsSByte(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetInt16Value(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out short _value, short _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsInt16(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetUInt16Value(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out ushort _value, ushort _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsUInt16(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetInt32Value(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out int _value, int _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsInt32(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetUInt32Value(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out uint _value, uint _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsUInt32(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetInt64Value(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out long _value, long _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsInt64(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetUInt64Value(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out ulong _value, ulong _fallback = 0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsUInt64(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetSingleValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out float _value, float _fallback = 0.0f)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsSingle(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetDoubleValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out double _value, double _fallback = 0.0)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsDouble(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetStringValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out string _value, string _fallback = "")
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsString(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryForceGetStringValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out string _value, string _fallback = "")
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.ForceAsString(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetByteArrayValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out byte[] _value, byte[] _fallback = null)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsByteArray(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetListValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out List<MessagePackObject> _value, List<MessagePackObject> _fallback = null)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsList(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }

        public static bool TryGetDictionaryValue(this Dictionary<MessagePackObject, MessagePackObject> _dict, string _key, out Dictionary<MessagePackObject, MessagePackObject> _value, Dictionary<MessagePackObject, MessagePackObject> _fallback = null)
        {
            MessagePackObject obj;

            if (_dict.TryGetValue(_key, out obj))
            {
                _value = obj.AsDictionary(_fallback);
                return true;
            }

            _value = _fallback;
            return false;
        }
        #endregion
    }

    public class MessagePackExtObject : MessagePackObject
    {
        private sbyte mExtType;

        public sbyte extType { get { return mExtType; } }

        public MessagePackExtObject(sbyte type, byte[] data)
        {
            if (type < 0)
                throw new ArgumentException("type should be >= 0");

            if (data == null)
                throw new ArgumentException("data is null");

            mExtType = type;
            mType = Type.Extension;

            mValue = data;
            mValueTypeCode = System.Type.GetTypeCode(typeof(MessagePackExtObject));
        }
    }

    public class MessagePackObject
    {
        public enum Type
        {
            Nil,
            Integer,
            Boolean,
            Float,
            Raw,
            List,
            Map,

            Extension,
            String,
            Binary,
        }

        protected object mValue;
        protected Type mType;
        protected TypeCode mValueTypeCode;

        public MessagePackObject() { mValue = null; mType = Type.Nil; mValueTypeCode = TypeCode.Empty; }

        public MessagePackObject(bool _value)
        {
            mValue = _value;
            mType = Type.Boolean;
            mValueTypeCode = TypeCode.Boolean;
        }

        public MessagePackObject(float _value)
        {
            mValue = _value;
            mType = Type.Float;
            mValueTypeCode = TypeCode.Single;
        }

        public MessagePackObject(double _value)
        {
            mValue = _value;
            mType = Type.Float;
            mValueTypeCode = TypeCode.Double;
        }

        public MessagePackObject(byte[] _value, long offset, long length, bool _legacy)
        {
            if (length > uint.MaxValue)
                throw new Exception("Size overflow!");

            byte[] buf = new byte[length];
            Array.Copy(_value, offset, buf, 0, length);

            mValue = buf;
            mType = (_legacy ? Type.Raw : Type.Binary);
            mValueTypeCode = System.Type.GetTypeCode(typeof(byte[]));
        }

        public MessagePackObject(string _value)
        {
            if (_value == null)
            {
                mType = Type.Nil;
                mValue = null;
                mValueTypeCode = TypeCode.Empty;
            }
            else
            {
                mType = Type.String;
                mValue = _value;
                mValueTypeCode = TypeCode.String;
            }
        }

        public MessagePackObject(sbyte _value)
        {
            mValue = _value;
            mType = Type.Integer;
            mValueTypeCode = TypeCode.SByte;
        }

        public MessagePackObject(short _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.Int16;
        }

        public MessagePackObject(int _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.Int32;
        }

        public MessagePackObject(long _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.Int64;
        }

        public MessagePackObject(byte _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.Byte;
        }

        public MessagePackObject(ushort _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.UInt16;
        }

        public MessagePackObject(uint _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.UInt32;
        }

        public MessagePackObject(ulong _value)
        {
            mType = Type.Integer;
            mValue = _value;
            mValueTypeCode = TypeCode.UInt64;
        }

        public MessagePackObject(List<MessagePackObject> list)
        {
            mType = Type.List;
            mValue = list;
            mValueTypeCode = System.Type.GetTypeCode(typeof(List<MessagePackObject>));
        }

        public MessagePackObject(Dictionary<MessagePackObject, MessagePackObject> _dict)
        {
            mType = Type.Map;
            mValue = _dict;
            mValueTypeCode = System.Type.GetTypeCode(typeof(Dictionary<MessagePackObject, MessagePackObject>));
        }

        public bool isList { get { return (mType == Type.List); } }
        public bool isMap { get { return (mType == Type.Map); } }
        public bool isInteger { get { return (mType == Type.Integer); } }
        public bool isBoolean { get { return (mType == Type.Boolean); } }
        public bool isFloat { get { return (mType == Type.Float); } }
        public bool isString { get { return (mType == Type.String); } }
        public bool isBinary { get { return (mType == Type.Binary); } }
        public bool isExtension { get { return (mType == Type.Extension); } }
        public bool isNil { get { return (mType == Type.Nil); } }

        public sbyte AsSByte(sbyte _fallback = 0)
        {
            try { _fallback = Convert.ToSByte(mValue); } catch (Exception) { }
            return _fallback;
        }

        public byte AsByte(byte _fallback = 0)
        {
            try { _fallback = Convert.ToByte(mValue); } catch (Exception) { }
            return _fallback;
        }

        public short AsInt16(short _fallback = 0)
        {
            try { _fallback = Convert.ToInt16(mValue); } catch (Exception) { }
            return _fallback;
        }

        public ushort AsUInt16(ushort _fallback = 0)
        {
            try { _fallback = Convert.ToUInt16(mValue); } catch (Exception) { }
            return _fallback;
        }

        public int AsInt32(int _fallback = 0)
        {
            try { _fallback = Convert.ToInt32(mValue); } catch (Exception) { }
            return _fallback;
        }

        public uint AsUInt32(uint _fallback = 0)
        {
            try { _fallback = Convert.ToUInt32(mValue); } catch (Exception) { }
            return _fallback;
        }

        public long AsInt64(long _fallback = 0)
        {
            try { _fallback = Convert.ToInt64(mValue); } catch (Exception) { }
            return _fallback;
        }

        public ulong AsUInt64(ulong _fallback = 0)
        {
            try { _fallback = Convert.ToUInt64(mValue); } catch (Exception) { }
            return _fallback;
        }

        public bool AsBoolean(bool _fallback = false)
        {
            try { _fallback = Convert.ToBoolean(mValue); } catch (Exception) { }
            return _fallback;
        }

        public float AsSingle(float _fallback = 0.0f)
        {
            try { _fallback = Convert.ToSingle(mValue); } catch (Exception) { }
            return _fallback;
        }

        public double AsDouble(double _fallback = 0.0)
        {
            try { _fallback = Convert.ToDouble(mValue); } catch (Exception) { }
            return _fallback;
        }

        public byte[] AsByteArray(byte[] _fallback = null)
        {
            try { _fallback = (byte[])mValue; } catch (Exception) { }
            return _fallback;
        }

        public string AsString(string _fallback = "")
        {
            try { _fallback = (string)mValue; } catch (Exception) { }
            return _fallback;
        }

        public string ForceAsString(string _fallback = "")
        {
            try { _fallback = mValue.ToString(); } catch (Exception) { }
            return _fallback;
        }

        public List<MessagePackObject> AsList(List<MessagePackObject> _fallback = null)
        {
            try { _fallback = (List<MessagePackObject>)mValue; } catch (Exception) { }
            return _fallback;
        }

        public Dictionary<MessagePackObject, MessagePackObject> AsDictionary(Dictionary<MessagePackObject, MessagePackObject> _fallback = null)
        {
            try { _fallback = (Dictionary<MessagePackObject, MessagePackObject>)mValue; } catch (Exception) { }
            return _fallback;
        }

        public T AsExtension<T>(T _fallback = null) where T : MessagePackExtObject
        {
            try { _fallback = (T)mValue; } catch (Exception) { }
            return _fallback;
        }

        public static implicit operator byte(MessagePackObject obj) { return obj.AsByte(); }
        public static implicit operator sbyte(MessagePackObject obj) { return obj.AsSByte(); }
        public static implicit operator short(MessagePackObject obj) { return obj.AsInt16(); }
        public static implicit operator ushort(MessagePackObject obj) { return obj.AsUInt16(); }
        public static implicit operator int(MessagePackObject obj) { return obj.AsInt32(); }
        public static implicit operator uint(MessagePackObject obj) { return obj.AsUInt32(); }
        public static implicit operator long(MessagePackObject obj) { return obj.AsInt64(); }
        public static implicit operator ulong(MessagePackObject obj) { return obj.AsUInt64(); }
        public static implicit operator bool(MessagePackObject obj) { return obj.AsBoolean(); }
        public static implicit operator float(MessagePackObject obj) { return obj.AsSingle(); }
        public static implicit operator double(MessagePackObject obj) { return obj.AsDouble(); }
        public static implicit operator byte[] (MessagePackObject obj) { return obj.AsByteArray(); }
        public static implicit operator List<MessagePackObject>(MessagePackObject obj) { return obj.AsList(); }
        public static implicit operator Dictionary<MessagePackObject, MessagePackObject>(MessagePackObject obj) { return obj.AsDictionary(); }

        public static implicit operator MessagePackObject(byte value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(sbyte value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(short value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(ushort value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(int value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(uint value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(long value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(ulong value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(bool value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(float value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(double value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(string value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(List<MessagePackObject> value) { return new MessagePackObject(value); }
        public static implicit operator MessagePackObject(Dictionary<MessagePackObject, MessagePackObject> value) { return new MessagePackObject(value); }

        public override bool Equals(object obj)
        {
            MessagePackObject o = obj as MessagePackObject;

            if (o != null)
                return (o.mType == mType && object.Equals(o.mValue, mValue));

            return false;
        }

        public override int GetHashCode()
        {
            return (mValue == null ? 0 : mValue.GetHashCode());
        }

        public override string ToString()
        {
            System.Type type = (mValue == null ? null : mValue.GetType());
            TypeCode code = (type == null ? TypeCode.Empty : System.Type.GetTypeCode(type));

            switch (code)
            {
                case TypeCode.Empty:
                    {
                        return "\"\"";
                    }
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    {
                        return mValue.ToString();
                    }
                case TypeCode.String:
                    {
                        return "\"" + mValue.ToString() + "\"";
                    }
                default:
                    {
                        StringBuilder sb = new StringBuilder();

                        if (type == typeof(byte[]))
                        {
                            byte[] b = this;

                            sb.Append('(');
                            for (int i = 0; i < b.Length; i++)
                            {
                                sb.Append("0x");
                                sb.Append(b[i].ToString("x2"));
                                sb.Append(' ');
                            }

                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(')');
                        }
                        else if (type == typeof(List<MessagePackObject>))
                        {
                            List<MessagePackObject> list = this;

                            sb.Append('[');

                            int i = 0;
                            for (; i < list.Count; i++)
                            {
                                sb.Append(list[i].ToString());
                                sb.Append(", ");
                            }

                            if (list.Count > 0)
                                sb.Remove(sb.Length - 2, 2);

                            sb.Append(']');

                        }
                        else if (type == typeof(Dictionary<MessagePackObject, MessagePackObject>))
                        {
                            Dictionary<MessagePackObject, MessagePackObject> _dict = this;

                            sb.Append('{');

                            foreach (KeyValuePair<MessagePackObject, MessagePackObject> kv in _dict)
                            {
                                sb.Append(kv.Key.ToString());
                                sb.Append(":");
                                sb.Append(kv.Value.ToString());
                                sb.Append(", ");
                            }

                            if (_dict.Count > 0)
                                sb.Remove(sb.Length - 2, 2);

                            sb.Append('}');
                        }

                        return sb.ToString();
                    }
            }
        }
    }

    public interface IMsgPackExtension
    {
        sbyte GetExtType();
        byte[] GetRawData();
    }

    #region Endian 暫時用不到

    public static class EndianHelper
    {
        public static byte[] GetBytesLE(this short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes, 0, 2);

            return bytes;
        }

        public static byte[] GetBytesLE(this ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes, 0, 2);

            return bytes;
        }

        public static byte[] GetBytesLE(this int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes, 0, 4);

            return bytes;
        }

        public static byte[] GetBytesLE(this uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes, 0, 4);

            return bytes;
        }

        public static byte[] GetBytesLE(this long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes, 0, 8);

            return bytes;
        }

        public static byte[] GetBytesLE(this ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes, 0, 8);

            return bytes;
        }

        public static short GetInt16LE(this byte[] value, int start, bool safe = false)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value, start, 2);

            short ret = BitConverter.ToInt16(value, start);

            if (!BitConverter.IsLittleEndian && safe)
                Array.Reverse(value, start, 2);

            return ret;
        }

        public static ushort GetUInt16LE(this byte[] value, int start, bool safe = false)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value, start, 2);

            ushort ret = BitConverter.ToUInt16(value, start);

            if (!BitConverter.IsLittleEndian && safe)
                Array.Reverse(value, start, 2);

            return ret;
        }

        public static int GetInt32LE(this byte[] value, int start, bool safe = false)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value, start, 4);

            int ret = BitConverter.ToInt32(value, start);

            if (!BitConverter.IsLittleEndian && safe)
                Array.Reverse(value, start, 4);

            return ret;
        }

        public static uint GetUInt32LE(this byte[] value, int start, bool safe = false)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value, start, 4);

            uint ret = BitConverter.ToUInt32(value, start);

            if (!BitConverter.IsLittleEndian && safe)
                Array.Reverse(value, start, 4);

            return ret;
        }

        public static long GetInt64LE(this byte[] value, int start, bool safe = false)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value, start, 8);

            long ret = BitConverter.ToInt64(value, start);

            if (!BitConverter.IsLittleEndian && safe)
                Array.Reverse(value, start, 8);

            return ret;
        }

        public static ulong GetUInt64LE(this byte[] value, int start, bool safe = false)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(value, start, 8);

            ulong ret = BitConverter.ToUInt64(value, start);

            if (!BitConverter.IsLittleEndian && safe)
                Array.Reverse(value, start, 8);

            return ret;
        }
    }

    #endregion

}
