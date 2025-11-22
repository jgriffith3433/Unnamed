using UnityEditor;

namespace ProceduralWorlds.Storm.Terrain
{
    public static class StormVTEditorUtility 
    {

        /// <summary>
        /// Imports the relevant pipeline shader package
        /// </summary>
        /// <param name="forceImport">Skip the check if we already have the package, and resintall it any</param>
        public static void ImportPipelinePackages(bool forceImport = false)
        {
            StormVTDefaults defaults = StormVTDefaults.Instance;
            if (defaults == null)
                return;
            
            
            StormVTEnums.StormVTPipeline currentPipeline = StormVTUtilities.GetCurrentPipeline();

            #region Remove All Shader Folders

            string shaderFolder = AssetDatabase.GetAssetPath(defaults.shaderFolder);
            if (!string.IsNullOrEmpty(shaderFolder))
            {
                // Attempt to remove BuiltIn Folder
                string builtInPath = $"{shaderFolder}/BuiltIn";
                string builtInFolderGUID = AssetDatabase.AssetPathToGUID(builtInPath);
                if (!string.IsNullOrEmpty(builtInFolderGUID))
                    AssetDatabase.DeleteAsset(builtInPath);
                
                // Attempt to Remove HDRP Folder
                string hdrpPath = $"{shaderFolder}/HDRP";
                string hdrpFolderGUID = AssetDatabase.AssetPathToGUID(hdrpPath);
                if (!string.IsNullOrEmpty(hdrpFolderGUID))
                    AssetDatabase.DeleteAsset(hdrpPath);
                // Attempt to Remove URP Folder
                string urpPath = $"{shaderFolder}/URP";
                string urpFolderGUID = AssetDatabase.AssetPathToGUID(urpPath);
                if (!string.IsNullOrEmpty(urpFolderGUID))
                    AssetDatabase.DeleteAsset(urpPath);
            }

            #endregion

            #region Import Package

            if (!defaults.packagesImported || forceImport)
            {
                string packagePath = null;
                switch (currentPipeline)
                {
                    case StormVTEnums.StormVTPipeline.BuiltIn:
                        packagePath = AssetDatabase.GetAssetPath(defaults.builtInPackage);
                        break;
                    case StormVTEnums.StormVTPipeline.URP:
                        packagePath = AssetDatabase.GetAssetPath(defaults.urpPackage);
                        break;
                    case StormVTEnums.StormVTPipeline.HDRP:
                        packagePath = AssetDatabase.GetAssetPath(defaults.hdrpPackage);
                        break;
                }

                if (!string.IsNullOrEmpty(packagePath))
                {
                    // Import the appropriate package
                    AssetDatabase.ImportPackage(packagePath, false);
                }

            }

            #endregion
        }
        
        
        /// <summary>
        /// Performs a package reimport if it the pipeline package hasn't been imported yet
        /// </summary>
        public static void PerformMaintenance()
        {

            StormVTDefaults defaults = StormVTDefaults.Instance;
            if (defaults == null)
                return;

            StormVTEnums.StormVTPipeline currentPipeline = StormVTUtilities.GetCurrentPipeline();

            switch(currentPipeline)
            {
                case StormVTEnums.StormVTPipeline.BuiltIn:
                    if(defaults.builtInVirtualTerrainMaterial == null)
                    {
                        ImportPipelinePackages(true);
                    }
                    break;
                case StormVTEnums.StormVTPipeline.URP:
                    if (defaults.urpVirtualTerrainMaterial == null)
                    {
                        ImportPipelinePackages(true);
                    }
                    break;

                case StormVTEnums.StormVTPipeline.HDRP:
                    if (defaults.hdrpVirtualTerrainMaterial== null)
                    {
                        ImportPipelinePackages(true);
                    }
                    break;
            }

            //ImportPipelinePackages(true);
        }
        
    }
}


