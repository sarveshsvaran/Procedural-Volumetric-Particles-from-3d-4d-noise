using UnityEngine;
using System.Collections;

public class AudioScript : MonoBehaviour {
    AudioSource audio;
    SurfaceCreator targetSurface;
    public float strength;
    public float[] spectrum = new float[256];

    public enum PlayMode { Microphone, Music };

    public PlayMode playMode;

    void Start()
    {
        Application.runInBackground = true;
        targetSurface = GetComponent<SurfaceCreator>();
        audio = GetComponent<AudioSource>();
        if (playMode == PlayMode.Microphone) {
            audio.clip = Microphone.Start(Microphone.devices[0], true, 1000, 44100);
            Debug.Log(Microphone.devices[0]);
            //Microphone.Start();            
        }
        audio.Play();
    }

    void Update()
    {
        if (playMode== PlayMode.Microphone)
        {
            if (!Microphone.IsRecording(Microphone.devices[0]))
            {
                audio.clip = Microphone.Start(Microphone.devices[0], true, 1000, 44100);
            }
        }
        audio.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        int i = 1;
        while (i < spectrum.Length - 1)
        {
            //Debug.DrawLine(Vector3.zero, new Vector3(0, spectrum[i - 1] * 10, 0), Color.white);
            targetSurface.offset.x += spectrum[i - 1]/strength;
            targetSurface.offset.y = spectrum[i+1]*strength ;
            i++;
        }
    }
}
