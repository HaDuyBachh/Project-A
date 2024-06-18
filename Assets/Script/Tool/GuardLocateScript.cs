using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class GuardLocateScript : MonoBehaviour
    {
        public SoldierGroup Group;
        public int id;
        public void New(SoldierGroup group, int id, Vector3 pos)
        {
            this.Group = group;
            this.id = id;
            this.transform.position = pos;
        }    
    }
}