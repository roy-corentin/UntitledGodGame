using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum DestinationType
{
    Random,
    Drinkable,
    Eat,
    Sleep,
    None
}

public enum EventType
{
    Random,
    SearchFood,
    Eat,
    SearchSleep,
    Sleep,
    SearchWater,
    Drink
}

public class Animal : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public DestinationType destinationType;
    public EventType eventType;

    // [Header("Statistiques de base")]
    // public float force;
    // public float energie;
    // public float sante;
    // public float sasiete;
    // public float hydratation;
    // public float aggressivite;

    // [Header("Reproduction")]
    // public float cycleReproduction;
    // public int nombreDePetits;

    // [Header("Environnement")]
    // public float temperature;
    // public bool presencePredateur;

    // [Header("Autres")]
    // public float age;
    // public bool estCarnivore;

    [HideInInspector] public int prefabIndex;
    public float RemainingDistance = 0;

    [Header("---- Animations ----")]
    public Animator animator;

    [Header("---- Déplacements ----")]
    public float angularSpeed;
    public float acceleration;
    public float stoppingDistance;
    public float moveSpeed;

    [Header("---- RandomMove ----")]
    public float maxMoveTime = 10;
    [HideInInspector] public bool isMoving;
    private float startMoveTime;
    public float chanceToDoNothing = 10;
    public bool IsOverTime => Time.time - startMoveTime > maxMoveTime;

    [Header("---- Soif ----")]
    [Range(0, 100)] public float thirstValue = 100;
    public float decreaseThirstPerSecond = 0.1f;
    public bool NeedToDrink => thirstValue <= 1;
    [HideInInspector] public bool isDrinking = false;
    public float drinkRefillSpeed = 10f;

    [Header("---- Faim ----")]
    [Range(0, 100)] public float hungerValue = 100;
    public float decreaseHungerPerSecond = 0.1f;
    public bool NeedToEat => hungerValue <= 1;

    [Header("---- Sommeil ----")]
    [Range(0, 100)] public float sleepValue = 100;
    public float decreaseSleepPerSecond = 0.1f;
    public bool NeedToSleep => sleepValue <= 1;

    public void SetupNavAgent()
    {
        if (navAgent != null) return;

        navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = angularSpeed;
        navAgent.acceleration = acceleration;
        navAgent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (ARtoVR.Instance.currentMode == GameMode.AR) return;
        if (navAgent == null) return;

        DecisionTree.Instance.Callback(this);

        if (animator)
            animator.SetFloat("Speed", navAgent.velocity.magnitude);

        UpdateValues();
    }

    public bool IsAtDestination()
    {
        if (navAgent == null) return false;
        if (!navAgent.hasPath) return false;

        RemainingDistance = navAgent.remainingDistance;
        return navAgent.remainingDistance <= stoppingDistance;
    }

    public void Disable()
    {
        if (navAgent != null) Destroy(navAgent);
        if (this.gameObject.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = false;
        animator.speed = 0;
    }

    public void Enable()
    {
        SetupNavAgent();
        if (this.gameObject.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        animator.speed = 1;
    }

    void UpdateValues()
    {
        if (thirstValue > 0 && !isDrinking) thirstValue -= decreaseThirstPerSecond * Time.deltaTime;
        if (hungerValue > 0) hungerValue -= decreaseHungerPerSecond * Time.deltaTime;
        if (sleepValue > 0) sleepValue -= decreaseSleepPerSecond * Time.deltaTime;
    }

    public void RandomMove()
    {
        startMoveTime = Time.time;
        SetRandomTarget();
        isMoving = true;
    }

    public void FindDrinkable()
    {
        DotCoord nearestWater = LocationManager.GetNearestDotOfType(gameObject, Biome.Water);
        MapGenerator.Dot waterDot = MapGenerator.Map.Instance.mapDots[nearestWater.x][nearestWater.y];
        SetTarget(waterDot.transform);
        destinationType = DestinationType.Drinkable;
    }

    public void Drink()
    {
        isDrinking = true;
        thirstValue += drinkRefillSpeed * Time.deltaTime;
        if (animator && !animator.GetBool("Drink")) animator.SetBool("Drink", true);
        if (thirstValue >= 99)
        {
            Debug.Log("Animal is not thirsty anymore");
            isDrinking = false;
            thirstValue = 100;
            if (animator) animator.SetBool("Drink", false);
            RemoveTarget();
        }
    }

    public void SetTarget(Transform target)
    {
        isMoving = true;
        navAgent.SetDestination(target.position);
    }

    public void RemoveTarget()
    {
        isMoving = false;
        navAgent.ResetPath();
        destinationType = DestinationType.None;
    }

    public void SetRandomTarget()
    {
        List<GameObject> elements = ElementsSpawner.Instance.elements;
        if (elements.Count == 0) return;

        if (Random.Range(0, 100) < chanceToDoNothing)
        {
            RemoveTarget();
            return;
        }

        GameObject element = elements[Random.Range(0, elements.Count)];
        SetTarget(element.transform);
        destinationType = DestinationType.Random;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Animal))]
public class AnimalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    // draw a gizmo in the scene view at target's position
    void OnSceneGUI()
    {
        Animal animal = (Animal)target;

        if (animal.navAgent == null) return;

        Handles.color = Color.red;
        Handles.DrawWireDisc(animal.navAgent.destination, Vector3.up, 1, 2);
    }
}

#endif