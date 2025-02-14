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

public enum AnimalFoodLevel
{
    One,
    Two,
    Three,
    Four,
    Five
}

public enum FoodType
{
    Herbivore,
    Carnivore
}

public class Animal : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public DestinationType destinationType;
    public EventType eventType;

    [HideInInspector] public int prefabIndex;
    public float RemainingDistance = 0;
    [HideInInspector] public bool forceDestination = false;

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
    [SerializeField] public bool NeedToEat => hungerValue <= 1;
    public FoodType foodType;
    [HideInInspector] public GameObject foodTarget;
    [HideInInspector] public bool isEating = false;
    public float eatSpeed = 10f;
    public AnimalFoodLevel thisFoodLevel;
    public List<AnimalFoodLevel> canEatLevels;

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

    void Awake()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100, LayerMask.GetMask("Ground")))
        {
            if (!this.gameObject.TryGetComponent(out CapsuleCollider capsuleCollider)) return;
            float height = capsuleCollider.height * transform.localScale.y;
            transform.position = new Vector3(hit.point.x, hit.point.y + height / 2, hit.point.z);
        }
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
        if (forceDestination) return true;
        if (navAgent == null) return false;
        if (!navAgent.hasPath) return false;

        RemainingDistance = navAgent.remainingDistance;
        return navAgent.remainingDistance <= stoppingDistance;
    }

    public void Disable()
    {
        if (navAgent != null) Destroy(navAgent);
        animator.speed = 0;
    }

    public void Enable()
    {
        SetupNavAgent();
        animator.speed = 1;
    }

    void UpdateValues()
    {
        if (thirstValue > 0 && !isDrinking) thirstValue -= decreaseThirstPerSecond * Time.deltaTime;
        if (hungerValue > 0 && !isEating) hungerValue -= decreaseHungerPerSecond * Time.deltaTime;
        if (sleepValue > 0) sleepValue -= decreaseSleepPerSecond * Time.deltaTime;
    }

    public void RandomMove()
    {
        startMoveTime = Time.time;
        SetRandomTarget();
        isMoving = true;
    }

    public void FindFood()
    {
        if (foodType == FoodType.Herbivore)
        {
            DotCoord nearestGrass = LocationManager.GetNearestDotOfType(gameObject, Biome.Forest);
            MapGenerator.Dot grassDot = MapGenerator.Map.Instance.mapDots[nearestGrass.x][nearestGrass.y];
            SetTarget(grassDot.transform);
            destinationType = DestinationType.Eat;
            // TODO Gérer quand y'a pas de nourriture
        }
        else if (foodType == FoodType.Carnivore)
        {
            List<AnimalTarget> targets = new();
            foreach (AnimalFoodLevel level in canEatLevels)
            {
                AnimalTarget animal = AnimalSpawner.Instance.GetNearestAnimalOfFoodLevel(gameObject, level);
                if (animal.animal == null) continue;
                targets.Add(animal);
            }

            if (targets.Count == 0) return;

            targets.Sort((a, b) => a.distance.CompareTo(b.distance));

            SetTarget(targets[0].animal.transform);
            destinationType = DestinationType.Eat;
            foodTarget = targets[0].animal;
        }
    }

    public void Eat()
    {
        if (!isEating)
        {
            RemoveTarget();
            if (foodType == FoodType.Carnivore)
            {
                foodTarget.GetComponent<Animal>().Die();
                foodTarget = null;
            }
        }

        isEating = true;
        hungerValue += eatSpeed * Time.deltaTime;
        if (animator && !animator.GetBool("Eat")) animator.SetBool("Eat", true);
        if (hungerValue >= 99)
        {
            Debug.Log("Animal is not hungry anymore");
            isEating = false;
            hungerValue = 100;
            RemoveTarget();
        }
    }

    public void FindDrinkable()
    {
        DotCoord nearestWater = LocationManager.GetNearestDotOfType(gameObject, Biome.Water);
        MapGenerator.Dot waterDot = MapGenerator.Map.Instance.mapDots[nearestWater.x][nearestWater.y];
        SetTarget(waterDot.transform);
        destinationType = DestinationType.Drinkable;
        // TODO Gérer quand y'a pas de point d'eau
    }

    public void Drink()
    {
        if (!isDrinking) RemoveTarget();
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

    public void Die()
    {
        Debug.Log("Ho no, I'm dead");
        if (animator) animator.SetBool("Die", true);
        AnimalSpawner.Instance.spawnedAnimals.Remove(gameObject);
        Destroy(gameObject, 1);
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