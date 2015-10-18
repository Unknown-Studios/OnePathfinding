using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioScript : MonoBehaviour
{
    //This script is used for external alerting noises, for example an explosion.
    //If you want to use this script just attach it to the script that is going to make the sound.

    [Range(0, 1000)]
    public float SizeThreshold = 0f;

    private new AudioSource audio;
    private float audioValue;
    private float[] spectrum;

    private void AlertObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, audioValue * audio.maxDistance);
        foreach (Collider col in colliders)
        {
            if (col.transform == transform)
            {
                continue;
            }
            if (col.tag == "AI" && (SizeThreshold == 0f || col.GetComponent<AdvancedAI>().Size < SizeThreshold))
            {
                col.GetComponent<AdvancedAI>().Alert(col.gameObject, AdvancedAI.AlertType.Danger);
            }
        }
    }

    private float AnalyzeSound()
    {
        float max = 0;
        float[] samples = new float[1024];
        audio.GetOutputData(samples, 0);
        for (int i = 0; i < samples.Length; i++)
        {
            if (samples[i] > max)
            {
                max = samples[i];
            }
        }
        return max;
    }

    private void OnDrawGizmos()
    {
        if (audio != null && audioValue != 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, audioValue * audio.maxDistance);
        }
    }

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (audio.clip != null)
        {
            audioValue = AnalyzeSound();
            if (audioValue != 0.0f)
            {
                AlertObjects();
            }
        }
    }
}