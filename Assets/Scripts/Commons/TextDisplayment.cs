using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TamCam.Commons
{
    public class TextDisplayment : MonoBehaviour
    {
        static TextDisplayment instance;

        private TextDisplayment() { }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static TextDisplayment Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TextDisplayment>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(TextDisplayment).Name);
                        instance = go.AddComponent<TextDisplayment>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }


    }
}
