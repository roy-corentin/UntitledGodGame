using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;

public class RadialSelection : MonoBehaviour
{
    public OVRInput.Button spawnButton = OVRInput.Button.One;

    [Range(2, 10)]
    public int numberOfRadialPart = 5;
    public float angleOffset = 0f;
    public GameObject radialPartPrefab;
    public Transform canvas;
    [SerializeField] private List<GameObject> radialParts = new();
    public Transform handTransform;
    private int selectedRadialPart;
    public List<UnityEvent> OnRadialPartSelected;

    public void Start()
    {
        if (radialParts.Count != numberOfRadialPart)
        {
            radialParts.Clear();
            for (int i = 0; i < canvas.childCount; i++)
                radialParts.Add(canvas.GetChild(i).gameObject);
        }

#if UNITY_EDITOR
        Show();
#endif
    }

    public void Update()
    {
#if UNITY_EDITOR
        GetSelectedRadialPart();
        if (Input.GetKeyDown(KeyCode.H)) Show();
        if (Input.GetKeyDown(KeyCode.J)) HideAndTriggerSelected();
#endif
        if (OVRInput.GetDown(spawnButton)) Show();
        if (OVRInput.Get(spawnButton)) GetSelectedRadialPart();
        if (OVRInput.GetUp(spawnButton)) HideAndTriggerSelected();
    }

    public void Show()
    {
        canvas.gameObject.SetActive(true);
        canvas.SetPositionAndRotation(handTransform.position, handTransform.rotation);
    }

    public void HideAndTriggerSelected()
    {
        if (selectedRadialPart >= 0 && selectedRadialPart < OnRadialPartSelected.Count)
        {
            Debug.Log("Selected radial part: " + selectedRadialPart);
            OnRadialPartSelected[selectedRadialPart].Invoke();
        }
        canvas.gameObject.SetActive(false);
    }

    public void GetSelectedRadialPart()
    {
        Vector3 centerToHand = handTransform.position - canvas.position;
        Vector3 centerToHandProjected = Vector3.ProjectOnPlane(centerToHand, canvas.forward);
        float angle = Vector3.SignedAngle(canvas.up, centerToHandProjected, -canvas.forward);
        if (angle < 0) angle += 360f;

        if (centerToHand.magnitude < 0.05f) selectedRadialPart = -1;

        selectedRadialPart = (int)(angle * numberOfRadialPart / 360);
        for (int i = 0; i < numberOfRadialPart; i++)
            radialParts[i].transform.localScale = i == selectedRadialPart
                ? new Vector3(1.2f, 1.2f, 1.2f)
                : new Vector3(1f, 1f, 1f);
    }


    public void SpawnRadialParts()
    {
        foreach (GameObject radialPart in radialParts)
        {
#if UNITY_EDITOR
            DestroyImmediate(radialPart);
#else
            Destroy(radialPart);
#endif
        }
        radialParts.Clear();

        for (int i = 0; i < numberOfRadialPart; i++)
        {
            float angle = -i * 360f / numberOfRadialPart - angleOffset / 2f;
            Vector3 radialPartEulerAngle = new(0, 0, angle);

            GameObject radialPart = Instantiate(radialPartPrefab, canvas);
            radialParts.Add(radialPart);

            radialPart.transform.position = canvas.position;
            radialPart.transform.localEulerAngles = radialPartEulerAngle;

            radialPart.GetComponent<Image>().fillAmount = (1f / numberOfRadialPart) - (angleOffset / 360f);
        }
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(RadialSelection))]
public class RadialSelectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RadialSelection animal = (RadialSelection)target;
        GUILayout.Space(10);

        if (GUILayout.Button("Spawn Radial Parts"))
        {
            animal.SpawnRadialParts();
        }
    }
}

#endif