using System;
using System.IO;
using UnityEngine;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// List of "Transform" data for items to display
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/TRS List")]
    public class StormTRSList : ScriptableObject
    {
        /// <summary>
        /// Offset to apply to positions for Editor Preview
        /// </summary>
        [Tooltip("Offset to apply to positions for Editor Preview")]
        public Vector3 PreviewOffset;
        /// <summary>
        /// Size of area to draw positions for Editor Preview
        /// </summary>
        [Tooltip("Size of area to draw positions for Editor Preview")]
        public Vector3 PreviewScale;
        /// <summary>
        /// List of all Transform data for this List
        /// </summary>
        [Tooltip("List of all Transform data for this List")]
        public StormUnityItemData[] Transforms;

        /// <summary>
        /// Action invoked when a single Transform changes with Index of item changed
        /// </summary>
        public Action<int> OnTransformChanged;
        /// <summary>
        /// Action invoked when TransformList has changed. (Reordered, Item Removed, or Item Added)
        /// </summary>
        public Action OnTransformListChanged;

        /// <summary>
        /// Saves this TransformList as a binary file at specified path
        /// </summary>
        /// <param name="filename">Path of file to create</param>
        public void Save(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            FileStream stream = File.Create(filename);
            Save(stream);
            stream.Close();
        }
        /// <summary>
        /// Saves this TransformList to a binary data stream
        /// </summary>
        /// <param name="stream">Data stream to append this TransformList to</param>
        public void Save(Stream stream)
        {
            stream.WriteByte(3); //file version
            stream.Write(BitConverter.GetBytes(Transforms.Length), 0, 4);
            for (int t = 0; t < Transforms.Length; ++t)
            {
                StormUnityItemData trs = Transforms[t];
                float3 rot = trs.Rotation.eulerAngles;
                //Write Translation. X,Y,Z as floats
                stream.Write(BitConverter.GetBytes(trs.Translation.x));
                stream.Write(BitConverter.GetBytes(trs.Translation.y));
                stream.Write(BitConverter.GetBytes(trs.Translation.z));
                //Write Rotation as Euler floats
                stream.Write(BitConverter.GetBytes(rot.x));
                stream.Write(BitConverter.GetBytes(rot.y));
                stream.Write(BitConverter.GetBytes(rot.z));
                //Write Scale. X,Y,Z as floats
                stream.Write(BitConverter.GetBytes(trs.Scale.x));
                stream.Write(BitConverter.GetBytes(trs.Scale.y));
                stream.Write(BitConverter.GetBytes(trs.Scale.z));
                //Write 'Extra' data as half
                stream.Write(BitConverter.GetBytes(trs.ExtraData.x));
                stream.Write(BitConverter.GetBytes(trs.ExtraData.y));
            }
        }
        /// <summary>
        /// Loads data from a binary file to this TransformList
        /// </summary>
        /// <param name="filename">Path of file to load from</param>
        /// <returns>True if load was successful</returns>
        public bool Load(string filename)
        {
            if (!File.Exists(filename))
                return false;
            FileStream stream = File.OpenRead(filename);
            bool ret = Load(stream);
            stream.Close();
            return ret;
        }
        /// <summary>
        /// Loads data from a binary data stream
        /// </summary>
        /// <param name="stream">Data stream to load from</param>
        /// <returns>True if load was successful</returns>
        public bool Load(Stream stream)
        {
            if (stream == null)
            {
                Debug.Log("Failed to load TransformList. Stream is null or empty.");
                return false;
            }

            int version = stream.ReadByte();
            if (version < 1 || version > 3)
            {
                Debug.Log("Failed to load TransformList. File Version '" + version + "' is invalid");
                return false;
            }
            byte[] buf = new byte[4];
            if (stream.Read(buf, 0, 4) != 4)
            {
                Debug.Log("Failed to load TransformList. Unexpected EOF.");
                return false;
            }
            int count = BitConverter.ToInt32(buf, 0);
            if (count < 0)
                count = 0;
            Array.Resize(ref Transforms, count);
            if (count > 0)
            {
                buf = new byte[44];
                for (int t = 0; t < count; ++t)
                {
                    if (stream.Read(buf, 0, 44) != 44)
                    {
                        Debug.Log("Failed to load TransformList. Unexpected EOF.");
                        return false;
                    }
                    StormUnityItemData trs = new StormUnityItemData {
                        Translation = math.float3(BitConverter.ToSingle(buf, 0), BitConverter.ToSingle(buf, 4), BitConverter.ToSingle(buf, 8)),
                        Rotation = quaternion.Euler(BitConverter.ToSingle(buf, 12), BitConverter.ToSingle(buf, 16), BitConverter.ToSingle(buf, 20)),
                        Scale = math.float3(BitConverter.ToSingle(buf, 24), BitConverter.ToSingle(buf, 28), BitConverter.ToSingle(buf, 32)),
                        ExtraData = new Vector2(version > 1 ? BitConverter.ToSingle(buf, 36) : 0, version > 2 ? BitConverter.ToSingle(buf, 40) : 0),
                    };
                    Transforms[t] = trs;
                }
            }
            return true;
        }

        public bool Matches(StormTRSList other)
        {
            if (other == this)
                return true;
            if (other == null || other.Transforms.Length != Transforms.Length)
                return false;
            for (int x = 0; x < Transforms.Length; ++x)
            {
                if (Transforms[x] != other.Transforms[x])
                    return false;
            }
            return true;
        }
    }
}