using System;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [Serializable]
    public struct StormFeaturesetSourceLayerInfo : IConvertible
    {
        [StormLayer("", StormLayerRemapMode.FEATURESET)]
        public string LayerName;
        [Range(0, 2)]
        public float Strength;

        public StormFeaturesetSourceLayerInfo(string name, float strength = 1)
        {
            LayerName = name;
            Strength = strength;
        }

        public static implicit operator StormSourceLayerInfo(StormFeaturesetSourceLayerInfo info)
        {
            return new StormSourceLayerInfo(info.LayerName, info.Strength);
        }
        public static implicit operator StormFeaturesetSourceLayerInfo(StormSourceLayerInfo info)
        {
            return new StormFeaturesetSourceLayerInfo(info.LayerName, info.Strength);
        }

        public static bool operator == (StormFeaturesetSourceLayerInfo a, StormFeaturesetSourceLayerInfo b)
        {
            return a.LayerName == b.LayerName && a.Strength == b.Strength;
        }
        public static bool operator != (StormFeaturesetSourceLayerInfo a, StormFeaturesetSourceLayerInfo b)
        {
            return a.LayerName != b.LayerName || a.Strength != b.Strength;
        }
        public override bool Equals(object obj)
        {
            if (obj is StormFeaturesetSourceLayerInfo b)
            {
                return this == b;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return LayerName.GetHashCode() ^ Strength.GetHashCode();
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Strength;
        }

        public override string ToString()
        {
            return LayerName;
        }

        public string ToString(IFormatProvider provider)
        {
            return LayerName;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public struct StormSourceLayerInfo
    {
        [StormLayer("", StormLayerRemapMode.HIDE)]
        public string LayerName;
        [Range(0, 2)]
        public float Strength;

        public StormSourceLayerInfo(string name, float strength = 1)
        {
            LayerName = name;
            Strength = strength;
        }
        public static bool operator == (StormSourceLayerInfo a, StormSourceLayerInfo b)
        {
            return a.LayerName == b.LayerName && a.Strength == b.Strength;
        }
        public static bool operator != (StormSourceLayerInfo a, StormSourceLayerInfo b)
        {
            return a.LayerName != b.LayerName || a.Strength != b.Strength;
        }
        public override bool Equals(object obj)
        {
            if (obj is StormSourceLayerInfo b)
            {
                return this == b;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return LayerName.GetHashCode() ^ Strength.GetHashCode();
        }

        public override string ToString()
        {
            return LayerName;
        }

        public string ToString(IFormatProvider provider)
        {
            return LayerName;
        }
    }
    public struct StormSourceIndexInfo
    {
        public int Index;
        public float Strength;

        public StormSourceIndexInfo(int index, float strength)
        {
            Index = index;
            Strength = strength;
        }


        public override string ToString()
        {
            return Index.ToString();
        }
    }
}