using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AI))]
public class AIEditor : Editor
{
    #region Fields

    private AI _target;

    private bool ShowData = true;
    private bool ShowSpecs;

    #endregion Fields

    #region Methods

    public override void OnInspectorGUI()
    {
        _target = (AI)target;

        GUILayout.Space(10f);

        ShowData = EditorGUILayout.Foldout(ShowData, "Animal Data: ");
        if (ShowData)
        {
            EditorGUILayout.LabelField("Animal Size: " + _target.Size);
            EditorGUILayout.LabelField("Has Path: " + (_target.hasPath));
            EditorGUILayout.LabelField("Animal State: " + _target.AIState);
            EditorGUILayout.LabelField("Time till automated noise: " + Mathf.Round(-1f * _target.TillNoise) + "s");
            if (_target.FlockAnimal)
            {
                EditorGUILayout.LabelField("Flock ID: " + _target.FlockID);
                EditorGUILayout.LabelField("Master: " + _target.IsMaster.ToString());
            }
        }

        GUILayout.Space(10f);
        ShowSpecs = EditorGUILayout.Foldout(ShowSpecs, "Animal Specifications: ");
        if (ShowSpecs)
        {
            _target.RandomAddSpeed = EditorGUILayout.Toggle(new GUIContent("Randomize Speed: ", "Adds a random number between -2 and 2 to the speed."), _target.RandomAddSpeed);
            _target.speed = EditorGUILayout.FloatField("Speed: ", _target.speed);
            _target.Damage = EditorGUILayout.FloatField("Damage: ", _target.Damage);
            _target.SmellDistance = EditorGUILayout.FloatField("Smell Distance: ", _target.SmellDistance);
            _target.ViewDistance = EditorGUILayout.FloatField("View Distance: ", _target.ViewDistance);
            if (_target.AlertSound != null)
            {
                _target.NoiseDistance = EditorGUILayout.FloatField(new GUIContent("Noise Distance", "The maximum distance at which the alert sound can be heard."), _target.NoiseDistance);
            }

            GUILayout.Space(5f);

            //_target.Flying = EditorGUILayout.Toggle("Flying Animal: ", _target.Flying);
            _target.Type = (AI.AnimalType)EditorGUILayout.EnumPopup("Animal Type: ", _target.Type);
            _target.FlockAnimal = EditorGUILayout.Toggle("Flock Animal: ", _target.FlockAnimal);
            if (_target.FlockAnimal)
            {
                _target.FlockMember = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Flock Member", "This is if you want a specific animal to be the flock member. (If you want a different looking Master) Leave blank if not."), _target.FlockMember, typeof(GameObject), false);
                _target.minFlockSize = EditorGUILayout.IntSlider(new GUIContent("Min Flock Size"), _target.minFlockSize, 2, 100);
                _target.maxFlockSize = EditorGUILayout.IntSlider(new GUIContent("Max Flock Size"), _target.maxFlockSize, _target.minFlockSize, 100);
            }
            _target.AlertSound = (AudioClip)EditorGUILayout.ObjectField("Alert Sound: ", _target.AlertSound, typeof(AudioClip), false);
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    #endregion Methods
}