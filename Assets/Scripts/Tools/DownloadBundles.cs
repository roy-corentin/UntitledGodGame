using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.Security.Cryptography;

public class DownloadBundles : MonoBehaviour
{
    [HideInInspector] public string bundlesFolderPath = default;
    [HideInInspector] public List<GameObject> loadedObjects = new();
    private readonly List<string> downloadedFiles = new();
    public static DownloadBundles Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        CreateApplicationFolders();
        StartCoroutine(LoadAllSavedAssetBundles());
    }

    void CreateApplicationFolders()
    {
        bundlesFolderPath = Path.Combine(Application.persistentDataPath, "Bundles");
        if (!Directory.Exists(bundlesFolderPath))
            Directory.CreateDirectory(bundlesFolderPath);
    }

    public void DownloadAssetBundle(string url)
    {
        StartCoroutine(DownloadAndSave(url));
    }

    private IEnumerator DownloadAndSave(string url)
    {
        if (downloadedFiles.Contains(url))
        {
            Debug.Log("AssetBundle déjà téléchargé.");
            AnimalUI.Instance.infoText.text = "AssetBundle déjà téléchargé.";
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SendWebRequest();

        while (!request.isDone)
        {
            Debug.Log("Progression du téléchargement : " + (request.downloadProgress * 100).ToString("F2") + "%");
            AnimalUI.Instance.infoText.text = "Téléchargement : " + (request.downloadProgress * 100).ToString("F2") + "%";
            yield return null;
        }

        yield return request;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erreur de téléchargement : " + request.error);
            AnimalUI.Instance.infoText.text = "Erreur lors du téléchargement.";
        }
        else
        {
            downloadedFiles.Add(url);
            PlayerPrefs.SetString("AssetBundleURL", $"{GetSavedURL()},{url}");
            PlayerPrefs.Save();

            string fileName = $"bundle_{System.DateTime.Now.Ticks}.assetbundle";
            string bundlePath = Path.Combine(bundlesFolderPath, fileName);

            string downloadedChecksum = CalculateChecksum(request.downloadHandler.data);
            if (CheckAllChecksum(downloadedChecksum))
            {
                Debug.Log("Le fichier téléchargé existe déjà avec le même checksum.");
                AnimalUI.Instance.infoText.text = "Le fichier téléchargé existe déjà.";
                yield break;
            }

            File.WriteAllBytes(bundlePath, request.downloadHandler.data);
            Debug.Log("AssetBundle téléchargé et enregistré localement à : " + bundlePath);
            AnimalUI.Instance.infoText.text = "Fichier téléchargé et enregistré localement.";

            StartCoroutine(LoadAssetBundle(bundlePath));
        }
    }

    private bool CheckAllChecksum(string checksum)
    {
        string[] files = Directory.GetFiles(bundlesFolderPath);
        foreach (string file in files)
        {
            if (checksum == CalculateChecksum(File.ReadAllBytes(file)))
                return true;
        }
        return false;
    }

    private string CalculateChecksum(byte[] data)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(data);
            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    public string GetSavedURL()
    {
        return PlayerPrefs.GetString("AssetBundleURL", "");
    }

    private IEnumerator LoadAssetBundle(string path)
    {
        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(path);
        yield return bundleRequest;

        AssetBundle bundle = bundleRequest.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("Échec du chargement de l'AssetBundle.");
            yield break;
        }

        AssetBundleRequest assetRequest = bundle.LoadAllAssetsAsync<GameObject>();
        yield return assetRequest;

        foreach (GameObject obj in assetRequest.allAssets)
        {
            AnimalSpawner.Instance.animalPrefabs.Add(obj);
            loadedObjects.Add(obj);
            Debug.Log("Objet ajouté à la liste : " + obj.name);
            AnimalUI.Instance.AddButton(obj.name, AnimalSpawner.Instance.animalPrefabs.Count - 1);
        }

        bundle.Unload(false);
    }

    private IEnumerator LoadAllSavedAssetBundles()
    {
        string savedUrl = GetSavedURL();
        if (string.IsNullOrEmpty(savedUrl))
        {
            Debug.Log("Aucun AssetBundle n'a été téléchargé.");
            yield break;
        }

        string[] files = Directory.GetFiles(bundlesFolderPath);
        foreach (string file in files)
            if (file.EndsWith(".assetbundle"))
                StartCoroutine(LoadAssetBundle(file));
    }

    public void DeleteAllAssetBundles()
    {
        // Supprimer tous les GameObjects instanciés
        foreach (GameObject obj in loadedObjects)
            Destroy(obj);
        loadedObjects.Clear();

        // get all files in the bundles folder
        string[] files = Directory.GetFiles(bundlesFolderPath);
        foreach (string file in files)
        {
            File.Delete(file);
            Debug.Log("Fichier AssetBundle supprimé : " + file);
        }

        // Supprimer l'URL sauvegardée
        PlayerPrefs.DeleteKey("AssetBundleURL");
        PlayerPrefs.Save();

        downloadedFiles.Clear();

        Debug.Log("Tous les AssetBundles ont été supprimés et déchargés.");

        // Supprimer tous les boutons
        for (int i = 2; i < AnimalUI.Instance.animalButtons.Count; i++)
            Destroy(AnimalUI.Instance.animalButtons[i]);
        AnimalUI.Instance.animalButtons.RemoveRange(2, AnimalUI.Instance.animalButtons.Count - 2);
    }
}

// add editor script
#if UNITY_EDITOR
[CustomEditor(typeof(DownloadBundles))]
public class DownloadBundlesEditor : Editor
{
    public string url;

    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying) return;

        base.OnInspectorGUI();
        DownloadBundles myScript = (DownloadBundles)target;

        // create input field for the URL
        url = EditorGUILayout.TextField("URL", url);

        // create button to download the asset bundle
        if (GUILayout.Button("Download Asset Bundle"))
            myScript.DownloadAssetBundle(url);

        // create button to delete all asset bundles
        if (GUILayout.Button("Delete All Asset Bundles"))
            myScript.DeleteAllAssetBundles();
    }
}
#endif

