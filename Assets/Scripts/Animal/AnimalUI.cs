using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalUI : MonoBehaviour
{
    public static AnimalUI Instance;
    public GameObject AnimalButtonPrefab;
    public Transform AnimalButtonParent;
    public List<GameObject> animalButtons = new();
    public TMP_InputField animalNameInput;
    public Transform handTransform;
    public GameObject animalUI;
    public TextMeshProUGUI infoText;

    private void Awake()
    {
        Instance = this;
    }

    public void DownloadAnimalBundle()
    {
        DownloadBundles.Instance.DownloadAssetBundle(animalNameInput.text);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) Toggle();

        if (!animalUI.activeSelf) return;

        if (animalUI.activeSelf && !IsInView())
            animalUI.SetActive(false);
    }

    private bool IsInView()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(animalUI.transform.position);
        return !(viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1);
    }

    public void AddButton(string animalName, int prefabIndex)
    {
        GameObject button = Instantiate(AnimalButtonPrefab, AnimalButtonParent);

        GameObject firstChild = button.transform.GetChild(0).gameObject;
        GameObject firstChildText = firstChild.transform.GetChild(0).gameObject;

        Button spawnBtn = firstChild.GetComponent<Button>();

        spawnBtn.onClick.AddListener(() => AnimalSpawner.Instance.SpawnAnimal(prefabIndex));

        TextMeshProUGUI buttonText = firstChildText.GetComponent<TextMeshProUGUI>();
        buttonText.text = animalName;

        animalButtons.Add(button);
    }

    public void Show()
    {
        animalUI.SetActive(true);
        animalUI.transform.SetPositionAndRotation(
            handTransform.position + handTransform.forward * 0.3f,
            Quaternion.LookRotation(Camera.main.transform.forward));
    }

    public void Hide()
    {
        animalUI.SetActive(false);
    }

    public void Toggle()
    {
        if (animalUI.activeSelf) Hide();
        else Show();
    }

    public void OpenKeyboard()
    {
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    public void CloseKeyboard()
    {
        TouchScreenKeyboard.hideInput = true;
    }
}