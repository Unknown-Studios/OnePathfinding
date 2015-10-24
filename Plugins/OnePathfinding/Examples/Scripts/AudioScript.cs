using UnityEngine;

/// <summary>
/// Used for external audio-sources that should affect the AI agents.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioScript : MonoBehaviour
{
    //This script is used for external alerting noises, for example an explosion.
    //If you want to use this script just attach it to the script that is going to make the sound.

    /// The size of the AI before it will be alerted by the sound.
    [Range(0, 1000)]
    public float SizeThreshold = 0f;

    /// <summary>
    /// A reference to the audio component.
    /// </summary>
    private new AudioSource audio;

    /// <summary>
    /// The maximum distance at which this sound can be heard.
    /// </summary>
    private float audioValue;

    /// <summary>
    /// </summary>
    private float[] spectrum;

    /// <summary>
    /// Alert the AI agents nearby.
    /// </summary>
    private void AlertObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, audioValue * audio.maxDistance);
        foreach (Collider col in colliders)
        {
            if (col.transform == transform)
            {
                continue;
            }
            if (col.GetComponent<AdvancedAI>() && (SizeThreshold == 0f || col.GetComponent<AdvancedAI>().Size < SizeThreshold))
            {
                col.GetComponent<AdvancedAI>().Alert(col.gameObject, AdvancedAI.AlertType.Danger);
            }
        }
    }

    /// <summary>
    /// Analyze the output for the audio-source to check if it should alert anything.
    /// </summary>
    /// <returns></returns>
    private float AnalyzeSound()
    {
        float max = 0;
        float[] samples = new float[1024];
        audio.GetOutputData(samples, 0);
        foreach (float t in samples)
        {
            if (t > max)
            {
                max = t;
            }
        }
        return max;
    }

    /// <summary>
    /// Draw the circle gizmo's.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (GridManager.ShowGizmo && audio != null && audioValue != 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, audioValue * audio.maxDistance);
        }
    }

    /// <summary>
    /// Called on the start of the component
    /// </summary>
    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Called once each frame.
    /// </summary>
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