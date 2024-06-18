using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public interface IWeaponState
    {
        /// <summary>
        /// Sử dụng để kiểm tra các điều kiện chuyển của Weapon.
        /// </summary>
        void CheckSwitchWeapon();
    }

}
