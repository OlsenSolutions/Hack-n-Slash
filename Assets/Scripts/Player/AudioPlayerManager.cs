using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerManager: MonoBehaviour
{
      private static AudioPlayerManager instance = null;
      private AudioSource audioS;

      private void Awake()
      {
          if (instance == null)
          { 
               instance = this;
               DontDestroyOnLoad(gameObject);
               return;
          }
          if (instance == this) return; 
          Destroy(gameObject);
      }

      void Start()
      {
         audioS = GetComponent<AudioSource>();
         audioS.Play();
      }
}