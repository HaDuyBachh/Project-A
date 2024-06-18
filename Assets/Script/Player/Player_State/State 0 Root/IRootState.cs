using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public interface IRootState
    { 
        /// <summary>
        /// Xử lí Vector trọng lực trong các Root State
        /// </summary>
        void HandleGravity();
    }
}
