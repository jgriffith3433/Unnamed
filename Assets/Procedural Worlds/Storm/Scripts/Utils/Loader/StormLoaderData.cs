using System;
using System.IO;
using UnityEngine;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Loader Data")]
    public class StormLoaderData : ScriptableObject
    {
        /// <summary>
        /// Position offset of this data in world space
        /// </summary>
        public float3 Position;
        /// <summary>
        /// Size of this data in world space
        /// </summary>
        public float3 Size;

        /// <summary>
        /// Array of tiles to load
        /// </summary>
        public StormLoaderItem[] Tiles;

        public static StormLoaderData Load(string filename)
        {
            if (Path.GetExtension(filename).ToLower() == ".asset")
                return StormUtils.LoadAsset<StormLoaderData>(filename);
            if (!File.Exists(filename))
                return null;
            using FileStream stream = File.OpenRead(filename);
            return Load(stream);
        }
        public static StormLoaderData Load(Stream stream)
        {
            int version = stream.ReadByte();
            if (version == 1)
            {
                byte[] buf = new byte[4];
                if (stream.Read(buf, 0, 4) < 4)
                {
                    Debug.LogError("Error loading LoaderData. Unexpected EOF reading length.");
                    return null;
                }
                int len = BitConverter.ToInt32(buf, 0);
                if (len <= 0)
                buf = new byte[len];
                if (stream.Read(buf, 0, len) < len)
                {
                    Debug.LogError("Error loading LoaderData. Unexpected EOF.");
                    return null;
                }
                StormLoaderData ret = new StormLoaderData();
                ret.Position.x = BitConverter.ToSingle(buf, 0);
                ret.Position.y = BitConverter.ToSingle(buf, 4);
                ret.Position.z = BitConverter.ToSingle(buf, 8);
                len = BitConverter.ToInt32(buf, 12);
                ret.Tiles = new StormLoaderItem[len];
                int pos = 12;
                for (int t = 0; t < len; ++t)
                {
                    StormLoaderItem tile = new StormLoaderItem();
                    tile.Offset.x = BitConverter.ToSingle(buf, pos);
                    tile.Offset.y = BitConverter.ToSingle(buf, pos + 4);
                    tile.Offset.z = BitConverter.ToSingle(buf, pos + 8);
                    tile.Extents.x = BitConverter.ToSingle(buf, pos + 12);
                    tile.Extents.y = BitConverter.ToSingle(buf, pos + 16);
                    tile.Extents.z = BitConverter.ToSingle(buf, pos + 20);
                    tile.DataType = ReadString(buf, ref pos);
                    if (tile.DataType == null)
                        return null;
                    tile.Path = ReadString(buf, ref pos);
                    if (tile.Path == null)
                        return null;
                    ret.Tiles[t] = tile;
                }
                return ret;
            }
            else
            {
                Debug.LogError("Error loading LoaderData. Unexpected Version: " + version);
                return null;
            }
        }
        public void Save(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            using FileStream stream = File.OpenWrite(filename);
            Save(stream);
        }
        public void Save(Stream stream)
        {
            stream.WriteByte(1); //version
            long startPos = stream.Position;
            stream.Write(new byte[] { 0, 0, 0, 0 }); //temp length value to be overwritten at end
            stream.Write(BitConverter.GetBytes(Position.x));
            stream.Write(BitConverter.GetBytes(Position.y));
            stream.Write(BitConverter.GetBytes(Position.z));
            stream.Write(BitConverter.GetBytes(Tiles.Length));
            for (int t = 0; t < Tiles.Length; ++t)
            {
                stream.Write(BitConverter.GetBytes(Tiles[t].Offset.x));
                stream.Write(BitConverter.GetBytes(Tiles[t].Offset.y));
                stream.Write(BitConverter.GetBytes(Tiles[t].Offset.z));
                stream.Write(BitConverter.GetBytes(Tiles[t].Extents.x));
                stream.Write(BitConverter.GetBytes(Tiles[t].Extents.y));
                stream.Write(BitConverter.GetBytes(Tiles[t].Extents.z));
                WriteString(stream, Tiles[t].DataType);
                WriteString(stream, Tiles[t].Path);
            }
            //now get our length and overwrite the length data
            long endPos = stream.Position;
            stream.Position = startPos;
            int len = (int)(endPos - startPos) - 4; //length of this data after the length value
            stream.Write(BitConverter.GetBytes(len));
            stream.Position = endPos;
        }
        static string ReadString(byte[] buf, ref int pos)
        {
            int len = BitConverter.ToInt32(buf, pos);
            pos += 4;
            if (len < 0)
            {
                Debug.LogError("Error reading string. Invalid Length: " + len);
                return null;
            }
            char[] chars = new char[len];
            for (int c = 0; c < len; ++c)
                chars[c] = BitConverter.ToChar(buf, pos + (c * 2));
            pos += len * 2;
            return new string(chars);
        }
        static void WriteString(Stream stream, string str)
        {
            int len = str.Length;
            stream.Write(BitConverter.GetBytes(len));
            for (int c = 0; c < len; ++c)
                stream.Write(BitConverter.GetBytes(str[c]));
        }
    }
}