using System.Collections;
using UnityEngine;

/// <summary>
/// This component is used as a listener for the AdvancedAI script, it will listen for nearby audio
/// sources and report back if the AI can "Hear" them.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AdvancedAI))]
public class Listener : MonoBehaviour
{
    /// <summary>
    /// The sound that the AI will make when it is going to alert an animal.
    /// </summary>
    [HideInInspector]
    public AudioClip AlertSound;

    /// <summary>
    /// Whether or not to make an automated noise now and then.
    /// </summary>
    [HideInInspector]
    public bool automatedNoise;

    /// <summary>
    /// The distance at which the noise can be heard.
    /// </summary>
    [HideInInspector]
    public float NoiseDistance;

    /// <summary>
    /// Time until the next automated noise should be made.
    /// </summary>
    [HideInInspector]
    public float TillNoise;

    /// <summary>
    /// The current noise that this AI is making.
    /// </summary>
    [HideInInspector]
    public float audioValue;

    /// <summary>
    /// A reference to the AudioSource.
    /// </summary>
    private new AudioSource audio;

    private AdvancedAI main;

    /// <summary>
    /// Alert all the surrounding objects, when the object makes a noise.
    /// </summary>
    private void AlertObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, audioValue * audio.maxDistance);
        foreach (Collider col in colliders)
        {
            if (GetComponent<Flocking>() && GetComponent<Flocking>().IsFlockMember(col.gameObject))
            {
                continue;
            }
            if (col.GetComponent<AdvancedAI>() && col.GetComponent<AdvancedAI>().Size < main.Size)
            {
                col.GetComponent<AdvancedAI>().Alert(gameObject, AdvancedAI.AlertType.Danger);
            }
        }
    }

    /// <summary>
    /// Used to analyze the sound to find out if it would alert other objects.
    /// </summary>
    /// <returns></returns>
    private float AnalyzeSound()
    {
        float max = 0.0f;
        if (audio.clip != null)
        {
            float[] samples = new float[128];
            audio.GetOutputData(samples, 0);
            foreach (float f in samples)
            {
                if (f > max)
                {
                    max = f;
                }
            }
        }
        return max;
    }

    /// <summary>
    /// An iterator used for making the automated noise.
    /// </summary>
    /// <returns>Nothing</returns>
    private IEnumerator NoiseMaker()
    {
        if (automatedNoise && AlertSound)
        {
            float LastNoise = 0f;//Reset LastNoise variable.
            float ran = Random.Range(0.0f, 100.0f); //Get a random number between 0 and 100
            while (true) //Loop
            {
                TillNoise = (Time.realtimeSinceStartup - ran) - LastNoise; //Countdown to make a noise.
                if (TillNoise <= 0) //Time to make a noise again.
                {
                    audio.clip = AlertSound; //The sound to play when alerting the other objects.
                    audio.Play(); //Play the audio.
                    LastNoise = Time.realtimeSinceStartup; //Reset the LastNoise variable
                    ran = Random.Range(0.0f, 600.0f); //Create a new TillNoise.
                }
                yield return null;
            }
        }
    }

    private void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.maxDistance = NoiseDistance;
        audio.minDistance = 0.1f;

        main = GetComponent<AdvancedAI>();

        StartCoroutine(NoiseMaker());
    }

    private void Update()
    {
        audioValue = AnalyzeSound();
        if (audioValue != 0.0f)
        {
            AlertObjects();
        }
    }
}