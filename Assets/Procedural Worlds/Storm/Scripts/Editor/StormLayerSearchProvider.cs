using System;
using System.Collections.Generic;
using ProceduralWorlds.Storm.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ProceduralWorlds.Storm.StormEditor
{
    public class StormLayerSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public string[] layers;
        public StormLayerType[] layerTypes;
        public Action<string> onSelectedCallback;
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> list = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Storm Data Layers"))
            };
            list.Add(new SearchTreeEntry(new GUIContent("(None)"))
            {
                level = 1,
                userData = ""
            });
            list.Add(new SearchTreeEntry(new GUIContent("Add New Layer..."))
            {
                level = 1,
                userData = "ADD_NEW_LAYER"
            });
            Array.Sort(layers);
            for (int j = 0; j < layers.Length; j++)
            {
                string layerName = layers[j];
                list.Add(new SearchTreeEntry(new GUIContent(layerName))
                {
                    level = 1,
                    userData = layerName
                });
            }
            
            // TODO : Manny : Because we are adding multiple layers to a category, there are duplicates
            // var layerTypeGroups = Enum.GetValues(typeof(StormLayerType)).Cast<StormLayerType>().ToList();
            //
            // for (int i = 0; i < layerTypeGroups.Count; i++)
            // {
            //     StormLayerType layerTypeGroup = layerTypeGroups[i];
            //     var groupTitle = ObjectNames.NicifyVariableName(layerTypeGroup.ToString());
            //     list.Add(new SearchTreeGroupEntry(new GUIContent(groupTitle), 1));
            //
            //     for (int j = 0; j < layers.Length; j++)
            //     {
            //         string layerName = layers[j];
            //         StormLayerType layerType = layerTypes[j];
            //         if (layerType.HasFlag(layerTypeGroup))
            //         {
            //             list.Add(new SearchTreeEntry(new GUIContent(layerName))
            //             {
            //                 level = 2,
            //                 userData = layerName
            //             });
            //         }
            //     }
            // }

            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            onSelectedCallback?.Invoke((string)SearchTreeEntry.userData);
            return true;
        }
    
        private static List<string> GetFlags(Enum input)
        {
            List<string> result = new List<string>();
            foreach (Enum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlag(value))
                {
                    result.Add(value.ToString());
                }
            }

            return result;
        }
    }
}