#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ProceduralWorlds.Storm.Wizard
{
    [Serializable]
    public class ListRequestData
    {
#if UNITY_EDITOR
        public ListRequest ListRequestInstance;

        public string SearchPackageID;
        public Action<bool, PackageCollection> CallbackAction;
#endif
    }

    /// <summary>
    /// Package utils class that allows us to check, add and remove packages in the project
    /// </summary>
    public class StormPackageUtils : MonoBehaviour
    {
        #region Package Callback Events

        public delegate void OnSearchReceived(bool searchSuccessful);
        public static OnSearchReceived onSearchReceived;

        public delegate void OnAddReceived(bool addSuccessful);
        public static OnAddReceived onAddReceived;

        public delegate void OnRemoveReceived(bool removedSuccessful);
        public static OnRemoveReceived onRemoveReceived;

        #endregion
        #region Request Instances
#if UNITY_EDITOR
        private static SearchRequest SearchRequestInstance;
        public static List<ListRequestData> ListRequestInstances = new List<ListRequestData>();
        private static AddRequest AddRequestInstance;
        private static RemoveRequest RemoveRequestInstance;
#endif
        private static bool RequestInProgress = false;

        #endregion
        #region Public Static Package Calls
        /// <summary>
        /// Searches your client installed packages to see if it has been installed
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="packageVersion">3 part version string e.g. 1.3.4</param>
        /// <param name="offlineMode"></param>
        public static void SearchForPackage(string packageName, string packageVersion, bool offlineMode)
        {
#if UNITY_EDITOR
            SearchRequestInstance = Client.Search(packageName, offlineMode);
            EditorApplication.update -= SearchUpdate;
            EditorApplication.update += SearchUpdate;
            RequestInProgress = true;
#endif
        }
        /// <summary>
        /// Searches the project to see if the package is installed.
        /// </summary>
        /// <param name="packageName"></param>
#if UNITY_EDITOR
        public static void SearchForPackage(string packageName, Action<bool, PackageCollection> callbackResult)
        {

            ListRequestInstances.Add(new ListRequestData
            {
                SearchPackageID = packageName,
                CallbackAction = callbackResult,
                ListRequestInstance = Client.List()
            });
            if (!RequestInProgress)
            {
                EditorApplication.update -= ListUpdate;
                EditorApplication.update += ListUpdate;
            }

            RequestInProgress = true;
        }
#endif

        /// <summary>
        /// Adds a new package to your client
        /// </summary>
        /// <param name="packageName"></param>
        public static void AddPackage(string packageName)
        {
#if UNITY_EDITOR
            if (RequestInProgress)
            {
                Debug.LogError("Request is still in progress, please wait till it finishes before you call another request.");
                return;
            }

            AddRequestInstance = Client.Add(packageName);
            EditorApplication.update -= AddUpdate;
            EditorApplication.update += AddUpdate;
            RequestInProgress = true;
#endif
        }
        /// <summary>
        /// Removes a package from your client
        /// </summary>
        /// <param name="packageName"></param>
        public static void RemovePackage(string packageName)
        {
#if UNITY_EDITOR
            if (RequestInProgress)
            {
                Debug.LogError("Request is still in progress, please wait till it finishes before you call another request.");
                return;
            }

            RemoveRequestInstance = Client.Remove(packageName);
            EditorApplication.update -= RemoveUpdate;
            EditorApplication.update += RemoveUpdate;
            RequestInProgress = true;
#endif
        }

        #endregion
        #region Private Static Package Update Callbacks

        /// <summary>
        /// Callback update that processes the package search
        /// </summary>
        private static void SearchUpdate()
        {
#if UNITY_EDITOR
            if (SearchRequestInstance.IsCompleted)
            {
                if (SearchRequestInstance.Status == StatusCode.Success)
                {
                    if (SearchRequestInstance.Result != null)
                    {
                        onSearchReceived?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogError("Package failed to be found. Make sure you are using a valid engine version.");
                        onSearchReceived?.Invoke(false);
                    }
                }
                else if (SearchRequestInstance.Status == StatusCode.Failure)
                {
                    Debug.LogError(SearchRequestInstance.Error.message);
                    onSearchReceived?.Invoke(false);
                }

                RequestInProgress = false;
                EditorApplication.update -= SearchUpdate;
            }
#endif
        }
        /// <summary>
        /// Callback update that process the package search via Client.List()
        /// </summary>
        private static void ListUpdate()
        {
#if UNITY_EDITOR
            for (int i = ListRequestInstances.Count - 1; i >= 0; i--)
            {
                ListRequestData data = ListRequestInstances[i];
                if (data != null)
                {
                    if (data.ListRequestInstance.IsCompleted)
                    {
                        if (data.ListRequestInstance.Status == StatusCode.Success)
                        {
                            if (data.ListRequestInstance.Result != null)
                            {
                                if (ContainsPackage(data.ListRequestInstance.Result, data.SearchPackageID))
                                {
                                    data.CallbackAction?.Invoke(true, data.ListRequestInstance.Result);
                                }
                                else
                                {
                                    data.CallbackAction?.Invoke(false, data.ListRequestInstance.Result);
                                }
                            }
                            else
                            {
                                Debug.LogError("Package failed to be found. Make sure you are using a valid engine version.");
                                data.CallbackAction?.Invoke(false, data.ListRequestInstance.Result);
                            }
                        }
                        else if (data.ListRequestInstance.Status == StatusCode.Failure)
                        {
                            Debug.LogError(data.ListRequestInstance.Error.message);
                            data.CallbackAction?.Invoke(false, data.ListRequestInstance.Result);
                        }

                        ListRequestInstances.RemoveAt(i);
                    }
                }
            }

            if (ListRequestInstances.Count <= 0)
            {
                RequestInProgress = false;
                EditorApplication.update -= ListUpdate;
            }
#endif
        }
        /// <summary>
        /// Callback update that processes the add package
        private static void AddUpdate()
        {
#if UNITY_EDITOR
            if (AddRequestInstance.IsCompleted)
            {
                if (AddRequestInstance.Status == StatusCode.Success)
                {
                    if (AddRequestInstance.Result != null)
                    {
                        Debug.Log("Package: " + AddRequestInstance.Result.packageId + " was installed successfully.");
                        onAddReceived?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogError("Package: " + AddRequestInstance.Result.packageId + " failed to install, maybe it could not be found, are you using the right engine version?");
                        onAddReceived?.Invoke(false);
                    }
                }
                else if (AddRequestInstance.Status == StatusCode.Failure)
                {
                    Debug.LogError(AddRequestInstance.Error.message);
                    onAddReceived?.Invoke(false);
                }

                RequestInProgress = false;
                EditorApplication.update -= AddUpdate;
            }
#endif
        }
        /// <summary>
        /// Callback update that processes the remove package
        /// </summary>
        private static void RemoveUpdate()
        {
#if UNITY_EDITOR
            if (RemoveRequestInstance.IsCompleted)
            {
                if (RemoveRequestInstance.Status == StatusCode.Success)
                {
                    Debug.Log("Package: " + RemoveRequestInstance.PackageIdOrName + " was removed successfully.");
                    onRemoveReceived?.Invoke(true);
                }
                else if (RemoveRequestInstance.Status == StatusCode.Failure)
                {
                    Debug.LogError(RemoveRequestInstance.Error.message);
                    onRemoveReceived?.Invoke(false);
                }

                RequestInProgress = false;
                EditorApplication.update -= AddUpdate;
            }
#endif
        }
#if UNITY_EDITOR
        /// <summary>
        /// Checks to see if a package exists from a package collection
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        private static bool ContainsPackage(PackageCollection packages, string packageId)
        {
            foreach (PackageInfo package in packages)
            {
                if (string.Equals(package.name, packageId))
                {
                    return true;
                }

                foreach (DependencyInfo dependencyInfo in package.resolvedDependencies)
                {
                    if (string.Equals(dependencyInfo.name, packageId))
                    {
                        return true;
                    }
                }
            }
 
            return false;
        }
#endif
#endregion
    }
}