using UnityEngine;
using System;
using System.IO;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [System.Serializable]
    public struct TreePrototypeData
    {
        public GameObject prefab;
        public float bendFactor;
        public int navMeshLod;

        public TreePrototypeData(TreePrototype tree)
        {
            prefab = tree.prefab;
            bendFactor = tree.bendFactor;
            navMeshLod = tree.navMeshLod;
        }

        public static implicit operator TreePrototypeData(TreePrototype tree)
        {
            return new TreePrototypeData(tree);
        }

        public static implicit operator TreePrototype(TreePrototypeData data)
        {
            return new TreePrototype() {
                prefab = data.prefab,
                bendFactor = data.bendFactor,
                navMeshLod = data.navMeshLod
            };
        }
    }
    [System.Serializable]
    public struct DetailPrototypeData
    {
        public DetailRenderMode renderMode;
        public Color dryColor;
        public Color healthyColor;
        public float holeEdgePadding;
        public float noiseSpread;
        public int noiseSeed;
        public float maxHeight;
        public float minHeight;
        public float maxWidth;
        public float minWidth;
        public Texture2D prototypeTexture;
        public GameObject prototype;
        public bool usePrototypeMesh;
        public bool useInstancing;

        public DetailPrototypeData(DetailPrototype detail)
        {
            renderMode = detail.renderMode;
            dryColor = detail.dryColor;
            healthyColor = detail.healthyColor;
            holeEdgePadding = detail.holeEdgePadding;
            noiseSpread = detail.noiseSpread;
            noiseSeed = detail.noiseSeed;
            maxHeight = detail.maxHeight;
            minHeight = detail.minHeight;
            maxWidth = detail.maxWidth;
            minWidth = detail.minWidth;
            prototypeTexture = detail.prototypeTexture;
            prototype = detail.prototype;
            usePrototypeMesh = detail.usePrototypeMesh;
            useInstancing = detail.useInstancing;
        }
        public static implicit operator DetailPrototypeData(DetailPrototype detail) => new DetailPrototypeData(detail);
        public static implicit operator DetailPrototype(DetailPrototypeData data) => new DetailPrototype() {
            dryColor = data.dryColor,
            healthyColor = data.healthyColor,
            holeEdgePadding = data.holeEdgePadding,
            maxHeight = data.maxHeight,
            maxWidth = data.maxWidth,
            minHeight = data.minHeight,
            minWidth = data.minWidth,
            noiseSeed = data.noiseSeed,
            noiseSpread = data.noiseSpread,
            prototype = data.prototype,
            prototypeTexture = data.prototypeTexture,
            renderMode = data.renderMode,
            useInstancing = data.useInstancing,
            usePrototypeMesh = data.usePrototypeMesh
        };
    }
    public class StormTerrainData
    {
        [HideInInspector]
        [SerializeField]
        public TreePrototypeData[] m_terrainTrees = null;
        [HideInInspector]
        [SerializeField]
        public DetailPrototypeData[] m_terrainDetails = null;


        public static bool HasData(UnityEngine.Terrain terrain)
        {
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(terrain.terrainData);
            path = Path.ChangeExtension(path, ".StormData.bytes");
            return File.Exists(path);
#else
            return false;
#endif
        }
        public static StormTerrainData GetData(UnityEngine.Terrain terrain)
        {
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(terrain.terrainData);
            path = Path.ChangeExtension(path, ".StormData.bytes");
            if (!File.Exists(path))
                return null;
            using (FileStream fStream = new FileStream(path, FileMode.Open))
                return FromStream(fStream);
#else
            return null;
#endif
        }
        public static void CreateData(StormTerrainData tData, UnityEngine.Terrain terrain)
        {
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(terrain.terrainData);
            path = Path.ChangeExtension(path, ".StormData.bytes");
            if (File.Exists(path))
            {
                bool overwrite = UnityEditor.EditorUtility.DisplayDialog("Overwrite?", "Terrain Data for Terrain " + terrain.terrainData.name + " already exists.\nDo you want to overwrite it?", "Yes", "No");
                if (overwrite)
                    File.Delete(path);
                else
                    return;
            }
            using (FileStream fStream = new FileStream(path, FileMode.Create))
                ToStream(tData, fStream);
#endif
        }

        public static StormTerrainData FromStream(Stream stream)
        {
            byte[] buf = new byte[4];
            if (stream.Read(buf, 0, 4) < 4)
            {
                Debug.LogError("Unexpected EOF reading from Stream. Expected >4 bytes.\nRemaining=" + (stream.Length - stream.Position) + " Pos=" + stream.Position);
                return null;
            }
            int totalLen = BitConverter.ToInt32(buf, 0);
            buf = new byte[totalLen];
            if (stream.Read(buf, 0, totalLen) < totalLen)
            {
                Debug.LogError("Unexpected EOF reading from Stream. Expected " + totalLen + " bytes.\nRemaining=" + (stream.Length - stream.Position) + " Pos=" + stream.Position);
                return null;
            }
#if UNITY_EDITOR
            int numTrees = BitConverter.ToInt32(buf, 0);
            int numDetails = BitConverter.ToInt32(buf, 4);
            StormTerrainData ret = new StormTerrainData();
            ret.m_terrainTrees = new TreePrototypeData[numTrees];
            ret.m_terrainDetails = new DetailPrototypeData[numDetails];
            int pos = 8;
            for (int t = 0; t < numTrees; ++t)
            {
                TreePrototypeData tree = new TreePrototypeData();
                string id = ReadString(buf, ref pos);
                if (id == null)
                    return null;
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
                tree.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                tree.bendFactor = BitConverter.ToSingle(buf, pos);
                pos += 4;
                tree.navMeshLod = BitConverter.ToInt32(buf, pos);
                pos += 4;
                ret.m_terrainTrees[t] = tree;
            }
            for (int t = 0; t < numDetails; ++t)
            {
                DetailPrototypeData detail = new DetailPrototypeData();
                string id = ReadString(buf, ref pos);
                if (id == null)
                    return null;
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
                detail.prototype = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                id = ReadString(buf, ref pos);
                if (id == null)
                    return null;
                path = UnityEditor.AssetDatabase.GUIDToAssetPath(id);
                detail.prototypeTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                detail.renderMode = (DetailRenderMode)BitConverter.ToInt32(buf, pos);
                pos += 4;
                detail.dryColor = new Color(BitConverter.ToSingle(buf, pos), BitConverter.ToSingle(buf, pos + 4), BitConverter.ToSingle(buf, pos + 8), BitConverter.ToSingle(buf, pos + 12));
                pos += 16;
                detail.healthyColor = new Color(BitConverter.ToSingle(buf, pos), BitConverter.ToSingle(buf, pos + 4), BitConverter.ToSingle(buf, pos + 8), BitConverter.ToSingle(buf, pos + 12));
                pos += 16;
                detail.holeEdgePadding = BitConverter.ToSingle(buf, pos);
                pos += 4;
                detail.noiseSpread = BitConverter.ToSingle(buf, pos);
                pos += 4;
                detail.noiseSeed = BitConverter.ToInt32(buf, pos);
                pos += 4;
                detail.maxHeight = BitConverter.ToSingle(buf, pos);
                pos += 4;
                detail.minHeight = BitConverter.ToSingle(buf, pos);
                pos += 4;
                detail.maxWidth = BitConverter.ToSingle(buf, pos);
                pos += 4;
                detail.minWidth = BitConverter.ToSingle(buf, pos);
                pos += 4;
                detail.usePrototypeMesh = (buf[pos] & 0x01) == 0x01;
                detail.useInstancing = (buf[pos] & 0x10) == 0x10;
                ++pos;
                ret.m_terrainDetails[t] = detail;
            }
            return ret;
#else
            //no sense loading anything if we are not in editor ... why would this be called if not in editor.
            //also I need UnityEditor classes to load prefab data
            return null;
#endif
        }
        public static void ToStream(StormTerrainData tData, Stream stream)
        {
#if UNITY_EDITOR
            long startPos = stream.Position;
            stream.Write(new byte[4] { 0,0,0,0 }); //write fake length to be overwritten later
            int streamLen = 0;
            stream.Write(BitConverter.GetBytes(tData.m_terrainTrees.Length));
            stream.Write(BitConverter.GetBytes(tData.m_terrainDetails.Length));
            streamLen += 8;
            for (int t = 0; t < tData.m_terrainTrees.Length; ++t)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(tData.m_terrainTrees[t].prefab);
                string id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                streamLen += WriteString(stream, id);
                stream.Write(BitConverter.GetBytes(tData.m_terrainTrees[t].bendFactor));
                stream.Write(BitConverter.GetBytes(tData.m_terrainTrees[t].navMeshLod));
                streamLen += 8;
            }
            for (int t = 0; t < tData.m_terrainDetails.Length; ++t)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(tData.m_terrainDetails[t].prototype);
                string id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                streamLen += WriteString(stream, id);
                path = UnityEditor.AssetDatabase.GetAssetPath(tData.m_terrainDetails[t].prototypeTexture);
                id = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                streamLen += WriteString(stream, id);
                stream.Write(BitConverter.GetBytes((int)tData.m_terrainDetails[t].renderMode)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].dryColor.r)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].dryColor.g)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].dryColor.b)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].dryColor.a)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].healthyColor.r)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].healthyColor.g)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].healthyColor.b)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].healthyColor.a)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].holeEdgePadding)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].noiseSpread)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].noiseSeed)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].maxHeight)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].minHeight)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].maxWidth)); //4
                stream.Write(BitConverter.GetBytes(tData.m_terrainDetails[t].minWidth)); //4
                stream.WriteByte((byte)((tData.m_terrainDetails[t].usePrototypeMesh ? 0x01 : 0x00) | (tData.m_terrainDetails[t].useInstancing ? 0x10 : 0x00))); //1
                streamLen += 65;
            }
            long endPos = stream.Position;
            stream.Position = startPos;
            stream.Write(BitConverter.GetBytes(streamLen));
            stream.Position = endPos;
#endif
        }
        /// <summary>
        /// Write a string to a Data Stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="str">String to write</param>
        public static int WriteString(Stream stream, string str)
        {
            if (str == null)
            {
                stream.Write(new byte[] { 0, 0, 0, 0 });
                return 4;
            }
            stream.Write(BitConverter.GetBytes(str.Length));
            for (int c = 0; c < str.Length; ++c)
            {
                stream.Write(BitConverter.GetBytes(str[c]));
            }
            return 4 + (2 * str.Length);
        }
        /// <summary>
        /// Read a string from a Data Stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>String read from Stream</returns>
        public static string ReadString(byte[] buf, ref int pos)
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
            {
                chars[c] = BitConverter.ToChar(buf, pos + c * 2);
            }
            pos += 2 * len;
            return new string(chars);
        }
    }
}