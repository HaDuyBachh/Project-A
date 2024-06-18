using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HaDuyBach
{

    public class Health_Bar : MonoBehaviour
    {
        public Slider slider;
        public Gradient gradient;
        public Image fill;
        private ThirdPersonController _player;
        private float _time_Hide;

        public void Awake()
        {
            _player = FindObjectOfType<ThirdPersonController>();
        }

        public void LateUpdate()
        {
            if (_time_Hide > 0.0f)
            {
                _time_Hide -= Time.deltaTime;
                transform.LookAt(_player.MainCamera);
                if (_time_Hide <= 0.0f) gameObject.SetActive(false);
            }
        }

        public void SetMaxHealth(int health)
        {
            slider.maxValue = health;
            slider.value = health;

            fill.color = gradient.Evaluate(1f);

            gameObject.SetActive(false);
        }
        public void SetHealth(int health)
        {
            slider.value = health;
            fill.color = gradient.Evaluate(slider.normalizedValue);
            _time_Hide = 3.0f;
            gameObject.SetActive(true);
        }
    }

}