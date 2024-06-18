using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class Item
    {
        public enum Type
        {
            none,   
            melee,
            pistol,
            rifle,
            smg,
            snip,
            shotgun,
            shiled,
            grenade,
            ammo,
            food,
            drink,
            money,
            quest,
            misc,
            junk,
        };

        /// <summary>
        /// Loại Item
        /// </summary>
        public Type type;
        /// <summary>
        /// Chỉ số để hiện trên tay
        /// </summary>
        public int index;
        /// <summary>
        /// Giới hạn chưa trong 1 slot
        /// </summary>
        public short limitPerSlot;
        /// <summary>
        /// Tốc độ tấn công của vũ khí
        /// </summary>
        public float speed;
        /// <summary>
        /// Sát thương của vũ khí
        /// </summary>
        public int damage;
        /// <summary>
        /// Số lượng đạn trong 1 băng đạn của vũ khí tầm xa
        /// </summary>
        public short magazine;
        /// <summary>
        /// Độ giật của sung
        /// </summary>
        public RecoilValue recoil;

        public Item()
        {
            type = Type.none;
            index = 0;
            limitPerSlot = 0;
        }
        /// <summary>
        /// dùng để lưu trữ 1 item bất kỳ chưa có thông số cụ thể
        /// </summary>
        /// <param name="type"> Loại đồ vật (tra ở Item.ItemType) </param>
        /// <param name="index"> Chỉ số để vật phẩm hiện thị trong Object Weapon_Block </param>
        /// <param name="limitPerSlot"> Giới hạn số lượng trên 1 ô </param>
        public Item(Type type, int index, short limitPerSlot)
        {
            this.type = type;
            this.index = index;
            this.limitPerSlot = limitPerSlot;
        }

        /// <summary>
        /// dùng để lưu trữ 1 vũ khí cận chiến
        /// </summary>
        /// <param name="type"> Loại đồ vật (tra ở Item.ItemType) </param>
        /// <param name="index"> Chỉ số để vật phẩm hiện thị trong Object Weapon_Block </param>
        /// <param name="limitPerSlot"> Giới hạn số lượng trên 1 ô </param>
        /// <param name="speed">Tốc độ bắn/đánh của vũ khí</param>
        /// <param name="damage">Sát thương vũ khí </param>
        /// <param name="ammo"> Số lượng đạn trong 1 băn của súng </param>
        public Item(Type type, int index, short limitPerSlot, float speed, int damage)
        {
            this.type = type;
            this.index = index;
            this.limitPerSlot = limitPerSlot;
            this.speed = speed;
            this.damage = damage;
        }

        /// <summary>
        /// dùng để lưu trữ 1 vũ khí tầm xa
        /// </summary>
        /// <param name="type"> Loại đồ vật (tra ở Item.ItemType) </param>
        /// <param name="index"> Chỉ số để vật phẩm hiện thị trong Object Weapon_Block </param>
        /// <param name="limitPerSlot"> Giới hạn số lượng trên 1 ô </param>
        /// <param name="speed">Tốc độ bắn/đánh của vũ khí</param>
        /// <param name="damage">Sát thương vũ khí </param>
        /// <param name="magazine"> Số lượng đạn trong 1 băn của súng </param>
        public Item(Type type, int index, short limitPerSlot, float speed, int damage,short magazine,RecoilValue recoil)
        {
            this.type = type;
            this.index = index;
            this.limitPerSlot = limitPerSlot;
            this.speed = speed;
            this.damage = damage;
            this.magazine = magazine;
            this.recoil = recoil;
        }

    }
}
