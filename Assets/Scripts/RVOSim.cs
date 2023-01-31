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
    private float defaultNeighbourDist = 15.0f;
    private int defaultMaxNeighbours = 10;
    private float defaultTimeHorizon = 5.0f;
    private float defaultTimeHorizonObst = 5.0f;
    private float defaultRadius = 1.0f;
    private float defaultMaxSpeed = 1.0f;
    private float defaultPersonalSpaceMultiplier = 1.0f;
    private float defaultLettingThroughMultiplier = 1.0f;
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

            float priority; // Dasja
            RVO.Vector2 goalPosition; // Dasja

            int agentGroupSize = 0;

            if (i < NrOfMovingAgents)
            {
                spawnPos = start1;
                goalPos = goal1;
                priority = 2.0f; // Dasja
                goalPosition = ToRVO(goal1); // Dasja
                isStatic = false;
                agentGroupSize = NrOfMovingAgents;
            }
            else if (i < NrOfMovingAgents + NrOfMovingAgents2)
            {
                spawnPos = goal1;
                goalPos = start1;
                priority = 2.0f; // Dasja
                goalPosition = ToRVO(goal2); // Dasja
                isStatic = false;
                agentGroupSize = NrOfMovingAgents2;
            }
            else
            {
                spawnPos = middle;
                goalPos = middle;
                priority = 1.0f; // Dasja
                goalPosition = ToRVO(middle); // Dasja
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
                                                defaultRadius, defaultMaxSpeed, new RVO.Vector2(0.0f, 0.0f), priority, goalPosition,
                                                defaultPersonalSpaceMultiplier, defaultLettingThroughMultiplier); // Dasja
            Simulator.Instance.setTimeStep(0.25f);
            Simulator.Instance.addAgent(ToRVO(allAgents[i].transform.position));

        }
    }

    //OPEN Timo
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
    //CLOSE Timo

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

            /*if (Simulator.Instance.isAgentStatic(i) && absDistToGoal < 400)
            {
                goalVel = new RVO.Vector2(0.001f, 0.001f);
            }*/
            if (absDistToGoal > 1) {
                goalVel = RVOMath.normalize(goalVel);
            }
            Simulator.Instance.setAgentPrefVelocity(i, goalVel);
            allAgents[i].transform.localPosition = ToUnity(agentPos);
        }

        Simulator.Instance.doStep();
    }

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

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using RVO;

// public class RVOSim : MonoBehaviour
// {
//    public int NrOfMovingAgents;
//    public int NrOfStaticAgents;
//    public GameObject GoalObj;
//    public GameObject StartObj;
//    public GameObject MiddleObj;
//    public GameObject AgentPrefab;
//    public GameObject AgentPrefab2;
//    Vector3 goal;
//    Vector3 start;
//    Vector3 middle;
//    GameObject[] allAgents;
//    int nrOfAgents;

//    Vector3[] goalPositions;


//    // Start is called before the first frame update
//    void Start()
//    {
//        System.Random ran = new System.Random();

//        goal = GoalObj.transform.position;
//        start = StartObj.transform.position;
//        middle = MiddleObj.transform.position;


//        nrOfAgents = NrOfMovingAgents + NrOfStaticAgents;
//        allAgents = new GameObject[nrOfAgents];

//        goalPositions = new Vector3[nrOfAgents];

//        float priority; // Dasja
//        RVO.Vector2 goalPosition; // Dasja

//        for (int i = 0; i < nrOfAgents; i++)
//        {
//            Vector3 spawnPos;
//            if (i < NrOfMovingAgents)
//            {
//                spawnPos = start;
//                priority = 2.0f; // Dasja
//                goalPosition = ToRVO(goal); // Dasja
//            }
//            else
//            {
//                spawnPos = middle;
//                priority = 1.0f; // Dasja
//                goalPosition = ToRVO(middle); // Dasja
//            }
          
//            spawnPos += new Vector3((float)ran.NextDouble() * 20 - 10, 0, (float)ran.NextDouble() * 20 - 10);

//            if (i < NrOfMovingAgents) {
//                 allAgents[i] = GameObject.Instantiate(AgentPrefab, spawnPos, Quaternion.identity);
//                 goalPositions[i] = goal;
//            }
//            else {
//                 allAgents[i] = GameObject.Instantiate(AgentPrefab2, spawnPos, Quaternion.identity);
//                 goalPositions[i] = spawnPos;
//            }
//            //allAgents[i] = GameObject.Instantiate(AgentPrefab, spawnPos, Quaternion.identity);

//            Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 1.0f, 1.0f, new RVO.Vector2(0.0f, 0.0f), priority, goalPosition); // Dasja

//            Simulator.Instance.setTimeStep(0.25f);
//            Simulator.Instance.addAgent(ToRVO(allAgents[i].transform.position));
//        }
//    }

//    RVO.Vector2 ToRVO(Vector3 v)
//    {
//        return new RVO.Vector2(v.x, v.z);
//    }

//    Vector3 ToUnity(RVO.Vector2 v)
//    {
//        return new Vector3(v.x(), 0, v.y());
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        for (int i = 0; i < Simulator.Instance.getNumAgents(); i++)
//        {
//            var agentPos = Simulator.Instance.getAgentPosition(i);

//            var goalPos = ToRVO(goalPositions[i]); //i < NrOfMovingAgents ? ToRVO(goal) : ToRVO(middle);

//            var goalVel = goalPos - agentPos;

//            if (RVOMath.absSq(goalVel) > 1)
//            {
//                goalVel = RVOMath.normalize(goalVel);
//            }

//            Simulator.Instance.setAgentPrefVelocity(i, goalVel);
//            allAgents[i].transform.localPosition = ToUnity(agentPos);
//        }

//        Simulator.Instance.doStep();
//    }
// }
