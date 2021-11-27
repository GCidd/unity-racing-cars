using System;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
[RequireComponent(typeof(ThirdPersonCharacter))]
public class EnemyMovement : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
    public ThirdPersonCharacter character { get; private set; }
    private Color defaultColor;
    public GameObject player;
    public float minDistanceToChase = 5;    // minimum distance to have from the player to start chasing them
    public float maxChaseDistance = 10; // maximum distance from the starting position to stop chasing the player
    private Vector3 targetPosition;
    private Vector3 startingPosition;
    private Vector3 positionBeforeChasing;
    private bool chasing = false;
    private bool returning = false;
    private bool joining = false;
    public bool joined { get; private set; }
    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        character = GetComponent<ThirdPersonCharacter>();
        defaultColor = transform.Find("EthanBody").GetComponent<Renderer>().material.color;

        agent.updateRotation = false;
        agent.updatePosition = true;
        startingPosition = transform.position;
        targetPosition = startingPosition;
        agent.Warp(startingPosition);
        joined = false;
    }

    public bool SawPlayer()
    {   // https://answers.unity.com/questions/1010169/how-to-know-if-an-object-is-looking-at-an-other.html
        Vector3 dirFromAtoB = (player.transform.position - transform.position).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, transform.forward);
        return dotProd >= 0.3 && dotProd <= 1.1f;
    }

    public bool EnemyInitiated()
    {
        return chasing && !joining;
    }

    private bool CloseEnoughToChase()
    {
        float distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
        return distanceFromPlayer <= minDistanceToChase;
    }

    private bool FarFromHome()
    {
        float distanceFromStartingPosition = Vector3.Distance(startingPosition, transform.position);
        return distanceFromStartingPosition > maxChaseDistance;
    }

    private void OnChaseStart()
    {
        agent.speed = 0.9f;
        transform.Find("EthanBody").GetComponent<Renderer>().material.color = Color.red;
    }

    private void OnReturnStart()
    {
        agent.speed = 1.5f;
        transform.Find("EthanBody").GetComponent<Renderer>().material.color = Color.blue;
    }

    private void OnWanderStart()
    {
        agent.speed = 0.5f;
        transform.Find("EthanBody").GetComponent<Renderer>().material.color = defaultColor;
    }

    private void OnJoin()
    {
        agent.speed = 5f;
    }

    public void OnJoined()
    {
        if (!joined)
        {
            GameObject.Find("SceneBroker").GetComponent<BattleStart>().SendMessage("AddJoinedName", gameObject);
        }
        joined = true;
    }

    public void Return()
    {
        joined = false;
        joining = false;
        chasing = false;
    }

    private void Update()
    {
        if (player == null)
            player = Populator.player;

        if (joining)
        {
            transform.Find("EthanBody").GetComponent<Renderer>().material.color = Color.magenta;

            if (agent.speed != 5f)
                agent.speed = 5f;

            if (joined)
                return;
            agent.SetDestination(player.transform.position);
            character.Move(agent.desiredVelocity, false, false);
            return;
        }

        if (CloseEnoughToChase() && SawPlayer() && !chasing && !returning)
        {   // start chasing
            OnChaseStart();
            chasing = true;
            positionBeforeChasing = transform.position;
        }
        if (chasing && FarFromHome())
        {   // if went too far from starting position start going back
            OnReturnStart();
            chasing = false;
            returning = true;
        }


        if (chasing)
        {
            agent.SetDestination(player.transform.position);
        }
        else if (returning)
        {
            agent.SetDestination(positionBeforeChasing);
        }
        else
        {
            if (targetPosition != null)
                agent.SetDestination(targetPosition);
        }

        if (agent.remainingDistance > agent.stoppingDistance)
            character.Move(agent.desiredVelocity, false, false);
        else
        {
            character.Move(Vector3.zero, false, false);
            returning = false;
        }
    }
        
    public void JoinFight()
    {
        OnJoin();
        joining = true;
    }

    public void SetTargetPosition(Vector3 target)
    {
        if (!chasing && !returning)
        {
            OnWanderStart();
            this.targetPosition = target;
        }
    }
}