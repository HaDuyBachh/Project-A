using UnityEngine;

namespace HaDuyBach
{ 
    public class OriginValue : MonoBehaviour
    {
        public Transform Origins;
        public Vector3 PositionOrigins;
        public Vector3 RotationOrigins;
        private void Awake()
        {
            Origins = transform;
        }
    }
}
