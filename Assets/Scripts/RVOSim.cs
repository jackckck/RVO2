using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;

public class RVOSim : MonoBehaviour
{
    public List<GameObject> Obstacles; //Timo
    public int NrOfMovingAgents;
    public int NrOfMovingAgents2;
    public int NrOfStaticAgents;
    public GameObject GoalObj;
    public GameObject StartObj; 
    public GameObject StartObj2;
    public GameObject GoalObj2;
    public GameObject MiddleObj;
    public GameObject StaticPrefab;
    public GameObject MovingPrefab;
    public GameObject StaticPrefab2;
    public GameObject MovingPrebab2;
    // agent default stats
    public float defaultNeighbourDist = 15.0f;
    public int defaultMaxNeighbours = 10;
    public float defaultTimeHorizon = 5.0f;
    public float defaultTimeHorizonObst = 5.0f;
    public float defaultRadius = 1.0f;
    public float defaultMaxSpeed = 1.0f;
    public float defaultPersonalSpaceMultiplier = 1.0f;
    Vector3 goal1;
    Vector3 start1;
    Vector3 goal2;
    Vector3 start2;
    Vector3 middle;
    GameObject[] allAgents;
    int nrOfAgents;

    Vector3[] spawnPositions;
    Vector3[] goalPositions;

    bool[] agentsStaticity;

   

    // Start is called before the first frame update
    void Start()
    {
        InitialiseObstacles(); //Timo

        System.Random ran = new System.Random();

        goal1 = GoalObj.transform.position;
        start1 = StartObj.transform.position;
        goal2 = GoalObj2.transform.position;
        start2 = StartObj2.transform.position;
        middle = MiddleObj.transform.position;

        nrOfAgents = NrOfMovingAgents + NrOfMovingAgents2 + NrOfStaticAgents;
        allAgents = new GameObject[nrOfAgents];

        spawnPositions = new Vector3[nrOfAgents];
        goalPositions = new Vector3[nrOfAgents];

        agentsStaticity = new bool[nrOfAgents];
        for (int i = 0; i < nrOfAgents; i++)
        {
            Vector3 spawnPos;
            Vector3 goalPos;

            bool isStatic;

            RVO.Vector2 goalPosition;

            int agentGroupSize = 0;

            if (i < NrOfMovingAgents)
            {
                spawnPos = start1;
                goalPos = goal1;
                goalPosition = ToRVO(goal1);
                isStatic = false;
                agentGroupSize = NrOfMovingAgents;
            }
            else if (i < NrOfMovingAgents + NrOfMovingAgents2)
            {
                spawnPos = goal1;
                goalPos = start1;
                goalPosition = ToRVO(goal2);
                isStatic = false;
                agentGroupSize = NrOfMovingAgents2;
            }
            else
            {
                spawnPos = middle;
                goalPos = middle;
                goalPosition = ToRVO(middle);
                isStatic = true;
                agentGroupSize = NrOfStaticAgents;
            }

            double spawnRadius = (double) (1.5f * ((float) Math.Sqrt(agentGroupSize * defaultRadius * defaultRadius)));
            double theta = ran.NextDouble() * (2 * Math.PI);
            double l = ran.NextDouble() * spawnRadius;

            float xDisplacement = (float) (l * Math.Cos(theta));
            float yDisplacement = (float) (l * Math.Sin(theta));

            Vector3 displacement = new Vector3(xDisplacement, 0, yDisplacement);
            spawnPos += displacement;
            goalPos += displacement;
            spawnPositions[i] = spawnPos;
            goalPositions[i] = goalPos;

            agentsStaticity[i] = isStatic;

            if (i < NrOfMovingAgents)
                allAgents[i] = GameObject.Instantiate(MovingPrebab2, spawnPos, Quaternion.identity);
            else if (i < NrOfMovingAgents + NrOfMovingAgents2)
                allAgents[i] = GameObject.Instantiate(StaticPrefab2, spawnPos, Quaternion.identity);
            else
                allAgents[i] = GameObject.Instantiate(StaticPrefab, spawnPos, Quaternion.identity);

            Simulator.Instance.setAgentDefaults(defaultNeighbourDist, defaultMaxNeighbours, defaultTimeHorizon, defaultTimeHorizonObst,
                                                defaultRadius, defaultMaxSpeed, new RVO.Vector2(0.0f, 0.0f), goalPosition,
                                                defaultPersonalSpaceMultiplier); // 
            Simulator.Instance.setTimeStep(0.25f);
            Simulator.Instance.addAgent(ToRVO(allAgents[i].transform.position));

        }
    }

    void InitialiseObstacles()
    {
        foreach (var parentObject in Obstacles) 
        {
            var obstacle = new List<RVO.Vector2>();
            foreach (Transform child in parentObject.transform)
            {
                var pos = child.position;
                obstacle.Add(ToRVO(pos));
            }
            Simulator.Instance.addObstacle(obstacle);
        }
        Simulator.Instance.processObstacles();
    }

    RVO.Vector2 ToRVO(Vector3 v)
    {
        return new RVO.Vector2(v.x, v.z);
    }

    Vector3 ToUnity(RVO.Vector2 v)
    {
        return new Vector3(v.x(), 0, v.y());
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < nrOfAgents; i++)
        {
            var agentPos = Simulator.Instance.getAgentPosition(i);
            RVO.Vector2 goalPos = ToRVO(goalPositions[i]);
            var goalVel = goalPos - agentPos;
            var absDistToGoal = RVOMath.absSq(goalVel);

            //UpdateAgentSkin(i);

            if (absDistToGoal > 1) {
                goalVel = RVOMath.normalize(goalVel);
            }
            Simulator.Instance.setAgentPrefVelocity(i, goalVel);
            allAgents[i].transform.localPosition = ToUnity(agentPos);
        }

        Simulator.Instance.doStep();
    }

    // alternates agents' colours depending on their state
    void UpdateAgentSkin(int i) {
        var agentPos = Simulator.Instance.getAgentPosition(i);
        bool isNowStatic = Simulator.Instance.isAgentStatic(i);
        if (agentsStaticity[i] != isNowStatic) {
            Destroy(allAgents[i]);
            GameObject newPrefab;
            if (i < NrOfMovingAgents + NrOfMovingAgents2)
            {
                newPrefab = isNowStatic ? StaticPrefab2 : MovingPrebab2;
            }
            else
            {
                newPrefab = isNowStatic ? StaticPrefab  : MovingPrefab;
            }
            allAgents[i] = GameObject.Instantiate(newPrefab, ToUnity(agentPos), Quaternion.identity);
            agentsStaticity[i] = isNowStatic;
        }
    }
}