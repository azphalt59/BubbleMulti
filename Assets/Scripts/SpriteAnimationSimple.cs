using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityCommon.Graphics
{
    public class SpriteAnimationSimple : MonoBehaviour
    {
        public bool IsPlaying = true;
        public bool Manual;


        public Sprite[] Sprites;
        public float Timer = 0.1f;

        public bool Loop;

        public GameObject ObjectToDestroyAtEnd;

        public bool RealTime;

        void Awake()
        {
            m_renderer = GetComponent<SpriteRenderer>();
            m_image = GetComponent<Image>();

            if (Loop)
                m_timer = (float)(Random.Range(0, Sprites.Length)) * Timer;

            m_previousTimer = Time.realtimeSinceStartup;
        }

        public void Play()
        {
            IsPlaying = true;
            m_timer = 0;
            m_previousIndex = -1;
        }

        void Update()
        {
            if (IsPlaying == false)
                return;

            if (Manual)
                return;

            int currentIndex = (int)(m_timer / Timer);

            if (currentIndex != m_previousIndex)
            {
                if (Loop == false && currentIndex >= Sprites.Length)
                {
                    if (ObjectToDestroyAtEnd != null)
                        GameObject.Destroy(ObjectToDestroyAtEnd);

                    IsPlaying = false;

                    return;
                }

                if (Loop)
                {
                    currentIndex = currentIndex % Sprites.Length;
                }

                m_previousIndex = currentIndex;

                if (m_renderer != null)
                    m_renderer.sprite = Sprites[currentIndex];
                else if (m_image != null)
                    m_image.sprite = Sprites[currentIndex];

            }

            if (RealTime == false)
                m_timer += Time.deltaTime;
            else
            {
                float offset = Time.realtimeSinceStartup - m_previousTimer;
                m_previousTimer = Time.realtimeSinceStartup;

                m_timer += offset;
            }
        }

        public void UpdateManual(int animationIndex, float timer)
        {
            int currentIndex = (int)(timer / Timer);

            if (currentIndex != m_previousIndex)
            {
                if (Loop == false && currentIndex >= Sprites.Length)
                {
                    if (ObjectToDestroyAtEnd != null)
                        GameObject.Destroy(ObjectToDestroyAtEnd);

                    IsPlaying = false;

                    return;
                }

                if (Loop)
                {
                    currentIndex = currentIndex % Sprites.Length;
                }

                m_previousIndex = currentIndex;

                if (m_renderer != null)
                    m_renderer.sprite = Sprites[currentIndex];
                else if (m_image != null)
                    m_image.sprite = Sprites[currentIndex];

            }

            m_timer += Time.deltaTime;
        }

        SpriteRenderer m_renderer;
        Image m_image;

        int m_previousIndex;
        float m_timer;
        float m_previousTimer;
    }

}