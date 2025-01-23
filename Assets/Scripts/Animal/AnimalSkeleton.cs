using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour
{
    private NavMeshAgent navAgent;

    [Header("Statistiques de base")]
    public float force;
    public float energie;
    public float vitesse;
    public float sante;
    public float sasiete;
    public float hydratation;
    public float aggressivite;

    [Header("Reproduction")]
    public float cycleReproduction;
    public int nombreDePetits;

    [Header("Environnement")]
    public float temperature;
    public bool presencePredateur;

    [Header("Autres")]
    public float age;
    public bool estCarnivore;
    public bool estEnMouvement;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {

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

    void SeDeplacer()
    {
        // Implémentation du déplacement
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
        if (navAgent == null)
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.SetDestination(target.position);
    }

    public void RemoveTarget()
    {
        if (navAgent == null)
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.ResetPath();
    }

    public void SetRandomTarget()
    {
        List<GameObject> elements = ElementsSpawner.Instance.elements;
        if (elements.Count == 0) return;

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

        if (GUILayout.Button("Set Target"))
            animal.SetRandomTarget();

        if (GUILayout.Button("Remove Target"))
            animal.RemoveTarget();

        GUILayout.EndHorizontal();
    }
}

#endif