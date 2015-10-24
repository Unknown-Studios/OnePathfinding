using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for the AdvancedAI component.
/// </summary>
[CustomEditor(typeof(AdvancedAI))]
public class AdvancedAIEditor : Editor
{
    //An instance of the AdvancedAI component.
    private AdvancedAI _target;

    //Show this AIs current audio settings
    private bool ShowAudio;

    //Show this AIs current behavior settings.
    private bool ShowBehave;

    //Show this AIs current Data.
    private bool ShowData = true;

    //Show this AIs current flock settings.
    private bool ShowFlock;

    //Show this AIs current flying settings.
    private bool ShowFly;

    //Show the current specifications for this AI.
    private bool ShowSpecs;

    /// <summary>
    /// Used to draw the inspector.
    /// </summary>
    public override void OnInspectorGUI()
    {
        _target = (AdvancedAI)target;

        GUILayout.Space(10f);

        GUIStyle style = EditorStyles.foldout;
        style.fontStyle = FontStyle.Bold;

        ShowData = EditorGUILayout.Foldout(ShowData, "Animal Data: ", style);
        if (ShowData)
        {
            EditorGUILayout.LabelField("Animal Size: " + _target.Size);
            EditorGUILayout.LabelField("Has Path: " + (_target.hasPath));
            EditorGUILayout.LabelField("Animal State: " + _target.AIState);
            if (_target.automatedNoise)
            {
                EditorGUILayout.LabelField("Time till automated noise: " + Mathf.Round(-1f * _target.TillNoise) + "s");
            }
            if (_target.FlockAnimal)
            {
                EditorGUILayout.LabelField("Flock ID: " + _target.FlockID);
                EditorGUILayout.LabelField("Master: " + _target.IsMaster.ToString());
            }
        }

        GUILayout.Space(10f);
        ShowBehave = EditorGUILayout.Foldout(ShowBehave, new GUIContent("Behave:", "Settings for how the AI behave"), style);
        if (ShowBehave)
        {
            _target.Type = (AdvancedAI.AnimalType)EditorGUILayout.EnumPopup("Animal Type: ", _target.Type);
            _target.speed = EditorGUILayout.FloatField("Speed: ", _target.speed);
            _target.RandomAddSpeed = EditorGUILayout.Toggle(new GUIContent("Randomize Speed: ", "Adds a random number between -2 and 2 to the speed."), _target.RandomAddSpeed);
            _target.Damage = EditorGUILayout.FloatField("Damage: ", _target.Damage);
            if (!_target.Flying)
            {
                _target.SmellDistance = EditorGUILayout.FloatField("Smell Distance: ", _target.SmellDistance);
            }
            _target.ViewDistance = EditorGUILayout.FloatField("View Distance: ", _target.ViewDistance);
            if (_target.AlertSound != null)
            {
                _target.NoiseDistance = EditorGUILayout.FloatField(new GUIContent("Noise Distance", "The maximum distance at which the alert sound can be heard."), _target.NoiseDistance);
            }
        }

        ShowFly = EditorGUILayout.Foldout(ShowFly, new GUIContent("Flying:", "Settings for the flying of an animal"));
        if (ShowFly)
        {
            _target.Flying = EditorGUILayout.Toggle("Flying animal:", _target.Flying);
            if (_target.Flying)
            {
                _target.Flyheight = EditorGUILayout.FloatField("Flying height:", _target.Flyheight);
            }
        }

        ShowFlock = EditorGUILayout.Foldout(ShowFlock, new GUIContent("Flock:", "settings for the AIs flock."), style);
        if (ShowFlock)
        {
            _target.FlockAnimal = EditorGUILayout.Toggle("Flock Animal: ", _target.FlockAnimal);
            if (_target.FlockAnimal)
            {
                _target.FlockMember = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Flock Member", "This is if you want a specific animal to be the flock member. (If you want a different looking Master) Leave blank if not."), _target.FlockMember, typeof(GameObject), false);
                _target.minFlockSize = EditorGUILayout.IntSlider(new GUIContent("Min Flock Size"), _target.minFlockSize, 2, 100);
                _target.maxFlockSize = EditorGUILayout.IntSlider(new GUIContent("Max Flock Size"), _target.maxFlockSize, _target.minFlockSize, 100);
            }
        }
        ShowAudio = EditorGUILayout.Foldout(ShowAudio, new GUIContent("Audio:", "Settings for the Audio"), style);
        if (ShowAudio)
        {
            _target.AlertSound = (AudioClip)EditorGUILayout.ObjectField("Alert Sound: ", _target.AlertSound, typeof(AudioClip), false);
            if (_target.AlertSound != null)
            {
                _target.automatedNoise = EditorGUILayout.Toggle(new GUIContent("Auto Noise", "Play alert sound from time to time."), _target.automatedNoise);
            }
        }
    }

    /// <summary>
    /// Update the inspector.
    /// </summary>
    private void OnInspectorUpdate()
    {
        Repaint();
    }
}