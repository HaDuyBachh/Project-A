using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HaDuyBach
{
    public class TestNavMesh : MonoBehaviour
    {
        public NavMeshAgent AI;
        private void Start()
        {
            AI = GetComponent<NavMeshAgent>();
            AI.SetDestination(Vector3.zero);
            AI.speed = 1.5f;
        }

        public void Update()
        {
                 
            
        }
    }
}
