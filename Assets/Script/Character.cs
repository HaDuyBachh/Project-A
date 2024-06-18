using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public abstract class Character : MonoBehaviour
    {
        #region organization
        /// <summary>
        /// thể hiện mã tổ chức mà nhân vật hoạt động
        /// </summary>
        public int OrgID = -1;
        #endregion

        /// <summary>
        /// Sát thương nhận vào 1 nhân vật (kể cả người chơi)
        /// </summary>
        /// <param name="Damage">Lượng sát thương</param>
        public abstract void Damaged(int Damage);
    }
}