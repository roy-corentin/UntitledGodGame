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
    Drink,
    Die
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

public enum SleepType
{
    Day,
    Night,
    None
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
    [HideInInspector] public bool randomMoveReachable = true;

    [Header("---- Soif ----")]
    [Range(0, 100)] public float thirstValue = 100;
    public float decreaseThirstPerSecond = 0.1f;
    public bool NeedToDrink => thirstValue <= 1;
    [HideInInspector] public bool isDrinking = false;
    public float drinkRefillSpeed = 10f;
    [HideInInspector] public bool waterSourceReachable = true;

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
    [HideInInspector] public bool foodSourceReachable = true;

    [Header("---- Sommeil ----")]
    [Range(0, 100)] public float sleepValue = 100;
    public float decreaseSleepPerSecond = 0.1f;
    public bool NeedToSleep => sleepValue <= 1;
    [HideInInspector] public bool isSleeping = false;
    public float sleepSpeed = 10f;
    public SleepType sleepType;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public readonly List<GameObject> notReachableElements = new();

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

        LockAnimation(true);
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
        if (sleepValue > 0 && !isSleeping) sleepValue -= decreaseSleepPerSecond * Time.deltaTime;
    }

    public void RandomMove()
    {
        if (!randomMoveReachable) return;
        if (IsAtDestination() && navAgent.hasPath) RemoveTarget();
        if (isMoving && !IsOverTime) return;

        startMoveTime = Time.time;
        SetRandomTarget();
    }

    public void FindFood()
    {
        if (foodType == FoodType.Herbivore)
        {
            MapGenerator.Dot nearestGrass = LocationManager.Instance.GetNearestDotOfType(gameObject, Biome.Forest, notReachableElements);
            if (nearestGrass == null)
            {
                foodSourceReachable = false;
                return;
            }

            if (!SetTarget(nearestGrass.transform)) return;
            destinationType = DestinationType.Eat;
        }
        else if (foodType == FoodType.Carnivore)
        {
            List<AnimalTarget> targets = new();
            foreach (AnimalFoodLevel level in canEatLevels)
            {
                AnimalTarget animal = AnimalSpawner.Instance.GetNearestAnimalOfFoodLevel(gameObject, level, notReachableElements);
                if (animal.animal == null) continue;
                targets.Add(animal);
            }

            if (targets.Count == 0)
            {
                foodSourceReachable = false;
                return;
            }

            targets.Sort((a, b) => a.distance.CompareTo(b.distance));

            if (!SetTarget(targets[0].animal.transform)) return;
            destinationType = DestinationType.Eat;
            foodTarget = targets[0].animal;
        }
    }

    public void Sleep()
    {
        if (!isSleeping) RemoveTarget();
        isSleeping = true;
        sleepValue += sleepSpeed * Time.deltaTime;
        if (animator && !animator.GetBool("Sleep")) animator.SetBool("Sleep", true);
        if (sleepValue >= 99)
        {
            isSleeping = false;
            sleepValue = 100;
            if (animator && animator.GetBool("Sleep")) animator.SetBool("Sleep", false);
            RemoveTarget();
        }
    }

    public void Eat()
    {
        if (!isEating)
        {
            RemoveTarget();
            if (foodType == FoodType.Carnivore)
            {
                foodTarget.GetComponent<Animal>().isDead = true;
                foodTarget = null;
            }
        }

        isEating = true;
        hungerValue += eatSpeed * Time.deltaTime;
        if (animator && !animator.GetBool("Eat")) animator.SetBool("Eat", true);
        if (hungerValue >= 99)
        {
            isEating = false;
            hungerValue = 100;
            if (animator && animator.GetBool("Eat")) animator.SetBool("Eat", false);
            RemoveTarget();
        }
    }

    public void FindDrinkable()
    {
        MapGenerator.Dot nearestWater = LocationManager.Instance.GetNearestDotOfType(gameObject, Biome.Water, notReachableElements);
        if (nearestWater == null)
        {
            waterSourceReachable = false;
            return;
        }

        if (!SetTarget(nearestWater.transform)) return;
        destinationType = DestinationType.Drinkable;
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
            if (animator && animator.GetBool("Drink")) animator.SetBool("Drink", false);
            RemoveTarget();
        }
    }

    public void Die()
    {
        if (animator.GetBool("Die")) return;

        if (animator && !animator.GetBool("Die")) animator.SetBool("Die", true);
        AnimalSpawner.Instance.spawnedAnimals.Remove(gameObject);
        RemoveTarget();
        Destroy(gameObject, 5);
        StatsManager.Instance.deadCount++;
    }

    public bool SetTarget(Transform target)
    {
        navAgent.SetDestination(target.position);

        NavMeshPath path = new();
        navAgent.CalculatePath(target.position, path);
        bool isReachable = !(path.status == NavMeshPathStatus.PathPartial)
                        && !(path.status == NavMeshPathStatus.PathInvalid);

        if (!isReachable)
        {
            Debug.LogWarning("Target is not reachable");
            notReachableElements.Add(target.gameObject);
            RemoveTarget();
            return false;
        }

        isMoving = true;
        return true;
    }

    public void RemoveTarget()
    {
        isMoving = false;
        navAgent.ResetPath();
        destinationType = DestinationType.None;
    }

    public void SetRandomTarget()
    {
        if (Random.Range(0, 100) < chanceToDoNothing)
        {
            RemoveTarget();
            return;
        }

        GameObject destination = LocationManager.Instance.GetRandomGroundPosition(notReachableElements);
        if (destination == null)
        {
            randomMoveReachable = false;
            isMoving = false;
            return;
        }

        if (!SetTarget(destination.transform)) return;
        destinationType = DestinationType.Random;
    }

    public void LockAnimation(bool status)
    {
        if (animator) animator.speed = status ? 0 : 1;
    }

    public void ResetReachableInfos()
    {
        notReachableElements.Clear();
        foodSourceReachable = true;
        waterSourceReachable = true;
        randomMoveReachable = true;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Animal))]
public class AnimalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying) return;

        // print all variables of animator + current animation
        Animal animal = (Animal)target;
        Animator animator = animal.animator;

        if (animator == null) return;

        // add space
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Animator", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current Animation", animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);

        EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            // horizontal layout
            EditorGUILayout.BeginHorizontal();
            if (parameter.type == AnimatorControllerParameterType.Float)
                EditorGUILayout.LabelField(parameter.name, animator.GetFloat(parameter.name).ToString());
            if (parameter.type == AnimatorControllerParameterType.Bool)
                EditorGUILayout.LabelField(parameter.name, animator.GetBool(parameter.name).ToString());
            EditorGUILayout.EndHorizontal();
        }
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