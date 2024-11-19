using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class CustomImporter_Odin : MonoBehaviour
{
    [FolderPath(AbsolutePath = true)]
    [Tooltip("Hedef klas�r� se�in")]
    [FoldoutGroup("Target Folder", expanded: true)]
    public string targetFolder = "";


    public string PickFolder = "";

    [Tooltip("Se�ilen klas�rdeki dosyalar")]
    [FoldoutGroup("Extarnall Assets", expanded: true)]
    public List<string> filePaths;

    private List<string> TargetFilePaths;


    [FoldoutGroup("Extarnall Assets", expanded: true)]
    [Button("Pick Asset Folder")]
    private void PickAssetFolder()
    {
        PickFolder = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");
        if (!string.IsNullOrEmpty(PickFolder))
        {
            // Unity Asset klas�r� yoluna g�re g�receli hale getir
            PickFolder = PickFolder.Replace(Application.dataPath, "").Replace("\\", "/");
            LoadFiles();
        }
    }

    [BoxGroup("Naming")]
    [Tooltip("Bulunacak metni girin")]
    public string findText = "";

    [BoxGroup("Naming")]
    [Tooltip("De�i�tirilecek metni girin")]
    public string replaceText = "";





    string targetPath = "";
    string folderPath = "";
    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Large)]
    private void Import()
    {
        folderPath = Path.Combine(Application.dataPath, Path.GetFileName(PickFolder));
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath); // Hedef klas�r yoksa olu�tur
        }

        foreach (var filePath in filePaths)
        {
            if (File.Exists(filePath)) // Dosyan�n varl���n� kontrol et
            {
                string fileName = Path.GetFileName(filePath);
                targetPath = Path.Combine(folderPath, fileName);

                try
                {
                    File.Copy(filePath, targetPath, true); // Dosyay� hedef klas�re kopyala
                    Debug.Log($"Dosya ta��nd�: {filePath} -> {targetPath}");
                    string unityAssetPath = targetPath.Replace(Application.dataPath, "Assets");
                    AssetDatabase.ImportAsset(unityAssetPath);


                }
                catch (Exception ex)
                {
                    Debug.LogError($"Dosya ta��n�rken hata olu�tu: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Dosya bulunamad�: {filePath}");
            }
        }

        AssetDatabase.Refresh(); // AssetDatabase'i g�ncelle
        if (!string.IsNullOrEmpty(targetFolder) && !string.IsNullOrEmpty(findText) && !string.IsNullOrEmpty(replaceText))
        {
            CheckTargetFiles();
        }

    }



    private void LoadFiles()
    {
        filePaths.Clear();

        if (Directory.Exists(PickFolder))
        {
            var files = Directory.GetFiles(PickFolder, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!file.EndsWith(".meta"))
                {
                    filePaths.Add(file.Replace(Application.dataPath, "Assets"));
                }
            }
        }
    }


    private void CheckTargetFiles()
    {
        TargetFilePaths.Clear();

        if (Directory.Exists(targetFolder))
        {
            var files = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (!file.EndsWith(".meta"))
                {
                    TargetFilePaths.Add(file.Replace(Application.dataPath, "Assets"));
                }
            }

            if (TargetFilePaths.Count > 0)
            {
                foreach (var filePath in TargetFilePaths)
                {
                    if (File.Exists(filePath)) // Dosyan�n varl���n� kontrol et
                    {
                        string fileName = Path.GetFileName(filePath);
                        string tPath = Path.Combine(folderPath, fileName);

                        try
                        {

                            string unityAssetPath = tPath.Replace(Application.dataPath, "Assets");

                            // Yeni dosya ad�n� olu�tur
                            string newFileName = Path.GetFileName(tPath).Replace(findText, replaceText);
                            string newUnityPath = Path.Combine(Path.GetDirectoryName(unityAssetPath), newFileName).Replace("\\", "/");

                            //De�i�tirilecek dosya ad� klas�rde varm� kontrol et
                            if (!File.Exists(newUnityPath))
                            {

                                File.Copy(filePath, tPath, true); // Dosyay� hedef klas�re kopyala
                                Debug.Log($"Dosya ta��nd�: {filePath} -> {tPath}");
                                AssetDatabase.ImportAsset(unityAssetPath);


                                // Dosya ad�n� de�i�tir
                                string error = AssetDatabase.RenameAsset(unityAssetPath, newFileName);
                                if (!string.IsNullOrEmpty(error))
                                {
                                    Debug.LogError($"Dosya ad� de�i�tirilirken hata olu�tu: {error}");
                                }
                                else
                                {
                                    Debug.Log($"Dosya ad� de�i�tirildi: {unityAssetPath} -> {newUnityPath}");
                                }
                            }


                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Dosya ta��n�rken hata olu�tu: {ex.Message}");
                        }
                    }
                }

                AssetDatabase.Refresh();

            }
        }
    }

    [Button("Clear Values")]
    private void ClearValues()
    {
        targetFolder = "";
        PickFolder = "";
        filePaths.Clear();
        TargetFilePaths.Clear();
        findText = "";
        replaceText = "";
        targetPath = "";
        folderPath = "";
    }

}
