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
    public GameObject AgentPrefab;
    public GameObject AgentPrefab2;
    public GameObject AgentPrefab3;
    Vector3 goal1;
    Vector3 start1;
    Vector3 goal2;
    Vector3 start2;
    Vector3 middle;
    GameObject[] allAgents;
    int nrOfAgents;

    Vector3[] spawnPositions;
    Vector3[] goalPositions;

    // Start is called before the first frame update
    void Start()
    {
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
        for (int i = 0; i < nrOfAgents; i++)
        {
            Vector3 spawnPos;
            Vector3 goalPos;

            float priority; // Dasja
            RVO.Vector2 goalPosition; // Dasja


            if (i < NrOfMovingAgents)
            {
                spawnPos = start1;
                goalPos = goal1;
                priority = 2.0f; // Dasja
                goalPosition = ToRVO(goal1); // Dasja
            }
            else if (i < NrOfMovingAgents + NrOfMovingAgents2)
            {
                spawnPos = start2;
                goalPos = goal2;
                priority = 2.0f; // Dasja
                goalPosition = ToRVO(goal2); // Dasja
            }
            else
            {
                spawnPos = middle;
                goalPos = middle;
                priority = 1.0f; // Dasja
                goalPosition = ToRVO(middle); // Dasja
            }

            Vector3 displacement = new Vector3((float)ran.NextDouble() * 20 - 10, 0, (float)ran.NextDouble() * 20 - 10);
            spawnPos += displacement;
            goalPos += displacement;
            spawnPositions[i] = spawnPos;
            goalPositions[i] = goalPos;

            if (i < NrOfMovingAgents)
                allAgents[i] = GameObject.Instantiate(AgentPrefab2, spawnPos, Quaternion.identity);
            else if (i < NrOfMovingAgents + NrOfMovingAgents2)
                allAgents[i] = GameObject.Instantiate(AgentPrefab3, spawnPos, Quaternion.identity);
            else
                allAgents[i] = GameObject.Instantiate(AgentPrefab, spawnPos, Quaternion.identity);

            Simulator.Instance.setTimeStep(0.25f);
            Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 1.0f, 1.0f, new RVO.Vector2(0.0f, 0.0f), priority, goalPosition); // Dasja

            InitialiseObstacles(); //Timo
        }        


        foreach (var agent in allAgents)
        {
            Simulator.Instance.addAgent(ToRVO(agent.transform.position));
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
        for (int i = 0; i < Simulator.Instance.getNumAgents(); i++)
        {
            var agentPos = Simulator.Instance.getAgentPosition(i);
            RVO.Vector2 goalPos = ToRVO(goalPositions[i]);
            var goalVel = goalPos - agentPos;

            if (RVOMath.absSq(goalVel) > 1)
                goalVel = RVOMath.normalize(goalVel);

            Simulator.Instance.setAgentPrefVelocity(i, goalVel);
            allAgents[i].transform.localPosition = ToUnity(agentPos);
        }

        Simulator.Instance.doStep();
    }
}



// Old RVOSim

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using RVO;

//public class RVOSim : MonoBehaviour
//{
//    public int NrOfMovingAgents;
//    public int NrOfStaticAgents;
//    public GameObject GoalObj;
//    public GameObject StartObj;
//    public GameObject MiddleObj;
//    public GameObject AgentPrefab;
//    Vector3 goal;
//    Vector3 start;
//    Vector3 middle;
//    GameObject[] allAgents;
//    int nrOfAgents;


//    // Start is called before the first frame update
//    void Start()
//    {
//        System.Random ran = new System.Random();

//        goal = GoalObj.transform.position;
//        start = StartObj.transform.position;
//        middle = MiddleObj.transform.position;


//        nrOfAgents = NrOfMovingAgents + NrOfStaticAgents;
//        allAgents = new GameObject[nrOfAgents];

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

//            allAgents[i] = GameObject.Instantiate(AgentPrefab, spawnPos, Quaternion.identity);

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

//            var goalPos = i < NrOfMovingAgents ? ToRVO(goal) : ToRVO(middle);

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
//}
