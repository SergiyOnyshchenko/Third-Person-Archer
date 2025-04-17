using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace YsoCorp {
    namespace GameUtils {

        public class YCPackageManager {

            public static string PACKAGES_URL = "https://gameutils-unity.ysocorp.com/public/unity/";

            public enum UpdatePackageType {
                Amazon = 1,
                Firebase = 2,
                ProdNetworks = 3,
                RatePopup = 101,
                IapTool = 201,
                I18nTool = 202
            }

            public static Dictionary<UpdatePackageType, PackageData> PACKAGES = new Dictionary<UpdatePackageType, PackageData>() {
                {UpdatePackageType.Amazon, new PackageData("2.0.0", "AmazonAPS", "AmazonAPS_") },
                {UpdatePackageType.Firebase, new PackageData("12.7.0", "Firebase/Analytics", "FirebaseAnalytics_") },
                {UpdatePackageType.ProdNetworks, new PackageData("1.4.0", "ProdNetworks", "ProdNetworks_") },
                {UpdatePackageType.RatePopup, new PackageData("1.1.0", "Packages/RatePopup", "RatePopup_") },
                {UpdatePackageType.IapTool, new PackageData("1.2.0", "Packages/IapTool", "IapTool_") },
                {UpdatePackageType.I18nTool, new PackageData("2.1.0", "Packages/I18nTool", "I18nTool_") },
            };

            public static IEnumerator DownloadPackage(string url, string fileName, Action<bool, string> onDownload = null) {
                if (fileName.EndsWith(".unitypackage") == false) {
                    fileName += ".unitypackage";
                }
                string path = Path.Combine(Application.temporaryCachePath, fileName);
                if (File.Exists(path) == false) {
                    var downloadHandler = new DownloadHandlerFile(path);

                    UnityWebRequest webRequest = new UnityWebRequest(url) {
                        method = UnityWebRequest.kHttpVerbGET,
                        downloadHandler = downloadHandler
                    };

                    var operation = webRequest.SendWebRequest();
                    Debug.Log("Downloading " + fileName);
                    while (!operation.isDone) {
                        yield return new WaitForSeconds(0.1f);
                    }

#if UNITY_2020_1_OR_NEWER
                    if (webRequest.result != UnityWebRequest.Result.Success)
#else
                    if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
                    {
                        Debug.LogError("The file " + fileName + " could not be downloaded.");
                        onDownload?.Invoke(false, path);
                        yield break;
                    }
                }
                onDownload?.Invoke(true, path);
            }

            public static void DownloadAndImportPackage(string url, string fileName, bool interactive, Action<bool, string> onDownload = null) {
                onDownload = ((downloaded, path) =>  AssetDatabase.ImportPackage(path, interactive)) + onDownload ;
                YCEditorCoroutine.StartCoroutine(DownloadPackage(url, fileName, onDownload));
                    
            }

            public static IEnumerator InstallPackage(string packageName, string version = "", Action onFinished = null, bool forceUpdate = false) {
                var pack = Client.List();
                while (!pack.IsCompleted) yield return null;

                bool isInstalled = pack.Result.FirstOrDefault(q => q.name == packageName) != null;
                UnityEditor.PackageManager.Requests.AddRequest packAdd = null;
                if (isInstalled == false || forceUpdate) {
                    if (string.IsNullOrEmpty(version) == false) {
                        packageName += "@" + version;
                    }
                    packAdd = Client.Add(packageName);
                }

                while (packAdd != null && !packAdd.IsCompleted) yield return null;
                onFinished?.Invoke();
            }

            #region Structures

            public struct PackageData {
                public Version version;
                public string folderPath;
                public string fileName;

                public PackageData(string version, string folderPath, string filePrefix) {
                    this.version = new Version(version);
                    this.folderPath = folderPath + "/";
                    this.fileName = filePrefix + version.Replace('.', '_');
                }
            }

            #endregion
        }
    }
}