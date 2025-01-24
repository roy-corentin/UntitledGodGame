using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour
{
    private NavMeshAgent navAgent;

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

    [Header("---- Animations ----")]
    public Animator animator;

    [Header("---- Déplacements ----")]
    public float angularSpeed;
    public float acceleration;
    public float stoppingDistance;
    public float moveSpeed;

    [Header("---- RandomMove ----")]
    public float maxMoveTime = 10;
    private bool isMoving;
    private float startMoveTime;
    public float chanceToDoNothing = 10;
    public bool IsOverTime => Time.time - startMoveTime > maxMoveTime;

    [Header("---- Soif ----")]
    [Range(0, 100)] public float thirstValue = 100;
    public float decreaseThirstPerSecond = 0.1f;
    public bool NeedToDrink => thirstValue <= 0;

    [Header("---- Faim ----")]
    [Range(0, 100)] public float hungerValue = 100;
    public float decreaseHungerPerSecond = 0.1f;
    public bool NeedToEat => hungerValue <= 0;

    [Header("---- Sommeil ----")]
    [Range(0, 100)] public float sleepValue = 100;
    public float decreaseSleepPerSecond = 0.1f;
    public bool NeedToSleep => sleepValue <= 0;

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

        if (navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            isMoving = false;
            RemoveTarget();
        }

        if (animator) animator.SetFloat("Speed", navAgent.velocity.magnitude);

        UpdateValues();

        if ((!isMoving || IsOverTime)
            && !NeedToDrink
            && !NeedToEat
            && !NeedToSleep)
            RandomMove();
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
        thirstValue -= decreaseThirstPerSecond * Time.deltaTime;
        hungerValue -= decreaseHungerPerSecond * Time.deltaTime;
        sleepValue -= decreaseSleepPerSecond * Time.deltaTime;
    }

    void RandomMove()
    {
        startMoveTime = Time.time;
        SetRandomTarget();
        isMoving = true;
    }

    void Courir()
    {
        // Implémentation du comportement de course
    }

    void Manger()
    {
        // Implémentation du comportement alimentaire
    }

    void Fuir()
    {
        // Implémentation du comportement de fuite
    }

    void Dormir()
    {
        // Implémentation du comportement de sommeil
    }

    void SAccoupler()
    {
        // Implémentation du comportement d'accouplement
    }

    void ChercherNourriture()
    {
        // Implémentation de la recherche de nourriture
    }

    void Chasser()
    {
        // Implémentation du comportement de chasse (pour les carnivores)
    }

    void Protéger()
    {
        // Implémentation du comportement de protection
    }

    void Boire()
    {
        // Implémentation du comportement pour boire
    }

    bool EnvironnementFavorable()
    {
        return true;
    }

    public void SetTarget(Transform target)
    {
        navAgent.SetDestination(target.position);
    }

    public void RemoveTarget()
    {
        navAgent.ResetPath();
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
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Animal))]
public class AnimalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Animal animal = (Animal)target;

        GUILayout.BeginHorizontal();

        // if (GUILayout.Button("Set Target"))
        //     animal.SetRandomTarget();

        // if (GUILayout.Button("Remove Target"))
        //     animal.RemoveTarget();

        GUILayout.EndHorizontal();
    }
}

#endif