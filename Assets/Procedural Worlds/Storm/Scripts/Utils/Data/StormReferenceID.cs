using System;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralWorlds.Storm.Utils
{
    [System.Serializable]
    public class StormReferenceSet
    {
        [SerializeField]
        [System.Obsolete("Use m_referenceGUIDs instead")]
        private StormReferenceID[] m_references;
        private StormReferenceGUID[] m_referenceGUIDs;

        [System.Obsolete("Use ReferenceGUIDs instead")]
        public System.ReadOnlySpan<StormReferenceID> References => m_references ?? new ReadOnlySpan<StormReferenceID>();
        public System.ReadOnlySpan<StormReferenceGUID> ReferenceGUIDs => m_referenceGUIDs ?? new ReadOnlySpan<StormReferenceGUID>();
        public int Count => m_referenceGUIDs?.Length??0;

        public StormReferenceSet(StormReferenceGUID[] ids)
        {
            m_referenceGUIDs = ids;
        }

        public StormReferenceSet(StormReferenceGUID[] ids, StormReferenceSet oldSet)
        {
            if (oldSet == null || oldSet.m_referenceGUIDs == null)
            {
                m_referenceGUIDs = ids;
                return;
            }
            int numNew = 0;
            for (int i = 0; i < ids.Length; ++i)
            {
                bool found = false;
                for (int r = 0; r < oldSet.m_referenceGUIDs.Length; ++r)
                {
                    if (oldSet.m_referenceGUIDs[r] == ids[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    ++numNew;
            }
            if (numNew == 0)
            {
                m_referenceGUIDs = new StormReferenceGUID[oldSet.m_referenceGUIDs.Length];
                oldSet.m_referenceGUIDs.CopyTo(m_referenceGUIDs, 0);
            }
            else
            {
                m_referenceGUIDs = new StormReferenceGUID[oldSet.m_referenceGUIDs.Length + numNew];
                oldSet.m_referenceGUIDs.CopyTo(m_referenceGUIDs, 0);
                int idx = oldSet.m_referenceGUIDs.Length;
                for (int i = 0; i < ids.Length; ++i)
                {
                    bool found = false;
                    for (int r = 0; r < oldSet.m_referenceGUIDs.Length; ++r)
                    {
                        if (oldSet.m_referenceGUIDs[r] == ids[i])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        m_referenceGUIDs[idx++] = ids[i];
                    }
                }
            }
        }

        public bool RemoveReference(StormReferenceGUID id)
        {
            for (int i = 0; i < m_referenceGUIDs.Length; ++i)
            {
                if (m_referenceGUIDs[i] == id)
                {
                    StormUtils.RemoveAt(ref m_referenceGUIDs, i);
                    return true;
                }
            }
            return false;
        }

        public bool AddReferences(StormReferenceGUID[] ids)
        {
            int numNew = 0;
            for(int i = 0; i < ids.Length; ++i)
            {
                bool found = false;
                for (int r = 0; r < m_referenceGUIDs.Length; ++r)
                {
                    if (m_referenceGUIDs[r] == ids[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    ++numNew;
            }
            if (numNew == 0)
                return false;
            int idx = m_referenceGUIDs.Length;
            System.Array.Resize(ref m_referenceGUIDs, idx + numNew);
            for(int i = 0; i < ids.Length; ++i)
            {
                bool found = false;
                for (int r = 0; r < m_referenceGUIDs.Length; ++r)
                {
                    if (m_referenceGUIDs[r] == ids[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    m_referenceGUIDs[idx++] = ids[i];
                }
            }
            return true;
        }

        [System.Obsolete("Use ReferenceGUIDs instead")]
        public bool SetOldReferences(ReadOnlySpan<StormReferenceID> ids)
        {
            m_references = new StormReferenceID[ids.Length];
            for (int i = 0; i < m_references.Length; ++i)
                m_references[i] = ids[i];
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj is StormReferenceSet b)
                return StormUtils.Match(m_referenceGUIDs, b.m_referenceGUIDs);
            return false;
        }
        public override int GetHashCode()
        {
            return m_referenceGUIDs.GetHashCode();
        }

        public static bool operator ==(StormReferenceSet a, StormReferenceSet b)
        {
            if ((object)a == null)
            {
                if ((object)b == null)
                    return true;
                return false;
            }
            else if ((object)b == null)
                return false;
            return a.Equals(b);
        }
        public static bool operator !=(StormReferenceSet a, StormReferenceSet b)
        {
            if ((object)a == null)
            {
                if ((object)b == null)
                    return false;
                return true;
            }
            else if ((object)b == null)
                return true;
            return !a.Equals(b);
        }

        public override string ToString()
        {
            System.Text.StringBuilder ret = new System.Text.StringBuilder("Count=").Append(Count);
            for (int i = 0; i < Count; ++i)
            {
                ret.Append('{').Append(m_referenceGUIDs[i].ToString()).Append('}');
            }
            return ret.ToString();
        }
    }
    [System.Serializable]
    public struct StormReferenceGUID
    {
        public uint4 SourceID;
        public uint4 DataID;
        public bool IsInstance;

        public StormReferenceGUID(System.Guid sourceID, System.Guid dataID, bool isInstance = false)
        {
            IsInstance = isInstance;
            byte[] data = sourceID.ToByteArray();
            SourceID = new uint4(System.BitConverter.ToUInt32(data, 0), System.BitConverter.ToUInt32(data, 4), System.BitConverter.ToUInt32(data, 8), System.BitConverter.ToUInt32(data, 12));
            data = dataID.ToByteArray();
            DataID = new uint4(System.BitConverter.ToUInt32(data, 0), System.BitConverter.ToUInt32(data, 4), System.BitConverter.ToUInt32(data, 8), System.BitConverter.ToUInt32(data, 12));
        }
        public StormReferenceGUID(uint4 sourceID, uint4 dataID, bool isInstance = false)
        {
            IsInstance = isInstance;
            SourceID = sourceID;
            DataID = dataID;
        }
        public override bool Equals(object obj)
        {
            return obj is StormReferenceGUID b && this == b;
        }
        public override int GetHashCode()
        {
            return SourceID.GetHashCode() ^ DataID.GetHashCode();
        }
        public static bool operator == (StormReferenceGUID a, StormReferenceGUID b)
        {
            return a.IsInstance == b.IsInstance && math.all(a.SourceID == b.SourceID) && math.all(a.DataID == b.DataID);
        }
        public static bool operator != (StormReferenceGUID a, StormReferenceGUID b)
        {
            return a.IsInstance != b.IsInstance && math.any(a.SourceID != b.SourceID) && math.any(a.DataID != b.DataID);
        }
        public string SourceString()
        {
            return new System.Text.StringBuilder(IsInstance ? "I" : "T").Append(SourceID.x.ToString("X2")).Append(SourceID.y.ToString("X2")).Append(SourceID.z.ToString("X2")).Append(SourceID.w.ToString("X2")).ToString();
        }
        public string DataString()
        {
            return new System.Text.StringBuilder(DataID.x.ToString("X2")).Append(DataID.y.ToString("X2")).Append(DataID.z.ToString("X2")).Append(DataID.w.ToString("X2")).ToString();
        }
        public override string ToString()
        {
            return new System.Text.StringBuilder(IsInstance ? "I" : "T").Append(SourceID.x.ToString("X2")).Append(SourceID.y.ToString("X2")).Append(SourceID.z.ToString("X2")).Append(SourceID.w.ToString("X2")).Append(DataID.x.ToString("X2")).Append(DataID.y.ToString("X2")).Append(DataID.z.ToString("X2")).Append(DataID.w.ToString("X2")).ToString();
        }
    }
    [System.Serializable]
    [System.Obsolete("Use StormReferenceGUID instead. kept for migration to new system.")]
    public struct StormReferenceID
    {
        const ulong INDEX_MASK      = 0xFFFF000000000000L;
        const ulong SOURCE_MASK     = 0x00007FFFFFFFFFFFL;
        const ulong INSTANCE_FLAG   = 0x0000800000000000L;
        const int INDEX_OFFSET = 48;

        public ulong ID;

        public bool IsInstance => (ID & INSTANCE_FLAG) == INSTANCE_FLAG;
        public ulong Source => (ID & SOURCE_MASK);
        public ushort Index => (ushort)((ID & INDEX_MASK) >> INDEX_OFFSET);

        public StormReferenceID(ulong id)
        {
            ID = id;
        }
        public StormReferenceID(ulong sourceID, ushort index)
        {
            ID = (sourceID & SOURCE_MASK) | (((ulong)index << INDEX_OFFSET) & INDEX_MASK);
        }

        public static bool operator==(StormReferenceID a, StormReferenceID b)
        {
            return a.ID == b.ID;
        }
        public static bool operator!=(StormReferenceID a, StormReferenceID b)
        {
            return a.ID != b.ID;
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is StormReferenceID id)
                return id.ID == ID;
            return false;
        }
    }
}