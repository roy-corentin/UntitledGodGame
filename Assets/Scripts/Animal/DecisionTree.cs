using System;
using UnityEngine;

#nullable enable
class Node
{
    public delegate bool Decision(Animal animal);
    public Decision? decision;
    public Node? yesNode;
    public Node? noNode;
    public Action<Animal> action;

    public Node()
    {
        decision = null;
        yesNode = null;
        noNode = null;
        action = DefaultAction;
    }

    public Node(Decision? decision, Node? yesNode, Node? noNode)
    {
        this.decision = decision;
        this.yesNode = yesNode;
        this.noNode = noNode;
        this.action = DefaultAction;
    }

    public Node(Decision? decision, Node? yesNode, Node? noNode, Action<Animal> action)
    {
        this.decision = decision;
        this.yesNode = yesNode;
        this.noNode = noNode;
        this.action = action;
    }

    public Node(Action<Animal> action)
    {
        this.decision = null;
        this.yesNode = null;
        this.noNode = null;
        this.action = action;
    }

    private void DefaultAction(Animal animal)
    {
        if (decision == null) return;
        bool value = decision(animal);
        if (value) yesNode?.action(animal);
        else noNode?.action(animal);
    }
}

#nullable disable

public class DecisionTree : MonoBehaviour
{
    public static DecisionTree Instance;

    Node root;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        Node Random = new((Animal animal) =>
        {
            animal.eventType = EventType.Random;
            if (animal.IsAtDestination() && animal.navAgent.hasPath) animal.RemoveTarget();
            if (animal.isMoving && !animal.IsOverTime) return;
            animal.RandomMove();
        });

        Node SearchFood = new((Animal animal) =>
        {
            animal.eventType = EventType.SearchFood;
            Debug.Log("SearchFood");
        });

        Node Eat = new((Animal animal) =>
        {
            animal.eventType = EventType.Eat;
            Debug.Log("Eat");
        });

        Node SearchSleep = new((Animal animal) =>
        {
            animal.eventType = EventType.SearchSleep;
            Debug.Log("SearchSleep");
        });

        Node Sleep = new((Animal animal) =>
        {
            animal.eventType = EventType.Sleep;
            Debug.Log("Sleep");
        });

        Node SearchWater = new((Animal animal) =>
        {
            animal.eventType = EventType.SearchWater;
            animal.FindDrinkable();
        });

        Node Drink = new((Animal animal) =>
        {
            animal.eventType = EventType.Drink;
            if (!animal.isDrinking)
            {
                Debug.Log("Start drinking");
                animal.RemoveTarget();
            }
            animal.Drink();
        });

        Node OnDrinkLocation = new((Animal animal) =>
            {
                return animal.isDrinking || animal.IsAtDestination();
            },
            Drink,
            SearchWater);

        Node OnEatLocation = new((Animal animal) =>
            {
                return false;
            },
            Eat,
            SearchFood);

        Node OnSleepLocation = new((Animal animal) =>
            {
                return false;
            },
            Sleep,
            SearchSleep);

        Node NeedToEat = new((Animal animal) =>
            {
                return animal.NeedToEat;
            },
            OnEatLocation,
            Random);

        Node NeedToSleep = new((Animal animal) =>
            {
                return animal.NeedToSleep;
            },
            OnSleepLocation,
            Random);

        Node NeedToDrink = new((Animal animal) =>
            {
                return animal.NeedToDrink || animal.isDrinking;
            },
            OnDrinkLocation,
            Random);

        root = NeedToDrink;
    }

    public void Callback(Animal animal)
    {
        root.action(animal);
    }
}