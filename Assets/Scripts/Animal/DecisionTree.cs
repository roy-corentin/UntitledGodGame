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

    public Node(Decision? decision, Node? yesNode, Node? noNode)
    {
        this.decision = decision;
        this.yesNode = yesNode;
        this.noNode = noNode;
        this.action = DefaultAction;
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
        Node Random = new(animal =>
        {
            animal.eventType = EventType.Random;
            if (animal.IsAtDestination() && animal.navAgent.hasPath) animal.RemoveTarget();
            if (animal.isMoving && !animal.IsOverTime) return;
            animal.RandomMove();
        });

        Node SearchFood = new(animal =>
        {
            animal.eventType = EventType.SearchFood;
            animal.FindFood();
        });

        Node Eat = new(animal =>
        {
            animal.eventType = EventType.Eat;
            animal.Eat();
        });

        Node SearchSleep = new(animal =>
        {
            animal.eventType = EventType.SearchSleep;
            Debug.Log("SearchSleep");
        });

        Node Sleep = new(animal =>
        {
            animal.eventType = EventType.Sleep;
            Debug.Log("Sleep");
        });

        Node SearchWater = new(animal =>
        {
            animal.eventType = EventType.SearchWater;
            animal.FindDrinkable();
        });

        Node Drink = new(animal =>
        {
            animal.eventType = EventType.Drink;
            animal.Drink();
        });

        Node OnDrinkLocation = new(animal =>
            {
                return animal.isDrinking || animal.IsAtDestination();
            },
            Drink,
            SearchWater);

        Node OnEatLocation = new(animal =>
            {
                return animal.isEating || animal.IsAtDestination();
            },
            Eat,
            SearchFood);

        Node OnSleepLocation = new(animal =>
            {
                return false;
            },
            Sleep,
            SearchSleep);

        Node NeedToEat = new(animal =>
            {
                return animal.NeedToEat || animal.isEating;
            },
            OnEatLocation,
            Random);

        Node NeedToSleep = new(animal =>
            {
                return animal.NeedToSleep;
            },
            OnSleepLocation,
            NeedToEat);

        Node NeedToDrink = new(animal =>
            {
                return animal.NeedToDrink || animal.isDrinking;
            },
            OnDrinkLocation,
            NeedToSleep);

        root = NeedToDrink;
    }

    public void Callback(Animal animal)
    {
        root.action(animal);
    }
}