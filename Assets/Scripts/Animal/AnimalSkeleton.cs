using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
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
}
