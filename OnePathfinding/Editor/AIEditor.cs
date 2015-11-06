using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for AdvancedAI.cs
/// </summary>
public class AIEditor : Editor
{
    /// <summary>
    /// Update the inspector.
    /// </summary>
    private void OnInspectorUpdate()
    {
        Repaint();
    }

    /// <summary>
    /// Custom editor for the AdvancedAI component.
    /// </summary>
    [CustomEditor(typeof(AdvancedAI))]
    public class AdvancedAIEditor : Editor
    {
        ///An instance of the AdvancedAI component.
        private AdvancedAI _target;

        ///Show this AIs current audio settings
        private bool ShowAudio;

        ///Show this AIs current behavior settings.
        private bool ShowBehave;

        ///Show this AIs current Data.
        private bool ShowData = true;

        ///Show this AIs current flock settings.
        private bool ShowFlock;

        ///Show this AIs current flying settings.
        private bool ShowFly;

        ///Show the current specifications for this AI.
        private bool ShowSpecs;

        ///Show the Additional options for this component.
        private bool ShowAdditional;

        private string[] options;

        /// <summary>
        /// Used to draw the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            _target = (AdvancedAI)target;
            if (options == null || options.Length != GridManager.Grids.Count)
            {
                options = new string[GridManager.Grids.Count];
                for (int i = 0; i < GridManager.Grids.Count; i++)
                {
                    options[i] = GridManager.Grids[i].name;
                }
            }

            EditorGUILayout.LabelField("Grid:");
            _target.GridIndex = EditorGUILayout.Popup(_target.GridIndex, options);
            _target.Pause = EditorGUILayout.Toggle("Pause: ", _target.Pause);

            GUILayout.Space(10f);

            GUIStyle style = EditorStyles.foldout;
            style.fontStyle = FontStyle.Bold;

            ShowData = EditorGUILayout.Foldout(ShowData, "Animal Data: ", style);
            string ps;
            if (_target.pt == AdvancedAI.PathType.none)
            {
                ps = "No path";
            }
            else if (_target.pt == AdvancedAI.PathType.HasPath)
            {
                ps = "Has path";
            }
            else
            {
                ps = "Requested path";
            }
            if (ShowData)
            {
                EditorGUILayout.LabelField("Animal Size: " + _target.Size);
                EditorGUILayout.LabelField("Path State: ", ps);
                EditorGUILayout.LabelField("Animal State: " + _target.AIState);
                if (_target.GetComponent<Listener>() && _target.GetComponent<Listener>().automatedNoise)
                {
                    EditorGUILayout.LabelField("Time till automated noise: " + Mathf.Round(-1f * _target.GetComponent<Listener>().TillNoise) + "s");
                }
                if (_target.GetComponent<Flocking>())
                {
                    EditorGUILayout.LabelField("Flock ID: " + _target.GetComponent<Flocking>().FlockID);
                    EditorGUILayout.LabelField("Master: " + _target.GetComponent<Flocking>().IsMaster.ToString());
                }
            }

            GUILayout.Space(10f);
            ShowBehave = EditorGUILayout.Foldout(ShowBehave, new GUIContent("Behave:", "Settings for how the AI behave"), style);
            if (ShowBehave)
            {
                _target.Type = (AdvancedAI.AnimalType)EditorGUILayout.EnumPopup("Animal Type: ", _target.Type);
                _target.speed = EditorGUILayout.FloatField("Speed: ", _target.speed);
                _target.RandomAddSpeed = EditorGUILayout.Toggle(new GUIContent("Randomize Speed: ", "Adds a random number between -2 and 2 to the speed."), _target.RandomAddSpeed);
                if (_target.Type == AdvancedAI.AnimalType.aggressive)
                {
                    _target.Damage = EditorGUILayout.FloatField("Damage: ", _target.Damage);
                }
                _target.ViewDistance = EditorGUILayout.FloatField("View Distance: ", _target.ViewDistance);
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

            ShowAdditional = EditorGUILayout.Foldout(ShowAdditional, new GUIContent("Components:", "Settings for the Components"), style);
            if (ShowAdditional)
            {
                EditorGUILayout.LabelField("Flocking:", EditorStyles.boldLabel);
                if (_target.GetComponent<Flocking>())
                {
                    _target.GetComponent<Flocking>().FlockMember = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Flock Member", "This is if you want a specific animal to be the flock member. (If you want a different looking Master) Leave blank if not."), _target.GetComponent<Flocking>().FlockMember, typeof(GameObject), false);
                    _target.GetComponent<Flocking>().minFlockSize = EditorGUILayout.IntSlider(new GUIContent("Min Flock Size"), _target.GetComponent<Flocking>().minFlockSize, 2, 100);
                    _target.GetComponent<Flocking>().maxFlockSize = EditorGUILayout.IntSlider(new GUIContent("Max Flock Size"), _target.GetComponent<Flocking>().maxFlockSize, _target.GetComponent<Flocking>().minFlockSize, 100);

                    if (GUILayout.Button("Remove"))
                    {
                        DestroyImmediate(_target.GetComponent<Flocking>(), true);
                    }
                }
                else
                {
                    if (GUILayout.Button("Add"))
                    {
                        _target.gameObject.AddComponent<Flocking>();
                    }
                }
                GUILayout.Space(10f);
                EditorGUILayout.LabelField("Listener:", EditorStyles.boldLabel);
                if (_target.GetComponent<Listener>())
                {
                    _target.GetComponent<Listener>().AlertSound = (AudioClip)EditorGUILayout.ObjectField("Alert Sound: ", _target.GetComponent<Listener>().AlertSound, typeof(AudioClip), false);
                    if (_target.GetComponent<Listener>().AlertSound != null)
                    {
                        _target.GetComponent<Listener>().NoiseDistance = EditorGUILayout.FloatField(new GUIContent("Noise Distance:", "The maximum distance at which noises from this animal can be heard."), _target.GetComponent<Listener>().NoiseDistance);
                        _target.GetComponent<Listener>().automatedNoise = EditorGUILayout.Toggle(new GUIContent("Auto Noise", "Play alert sound from time to time."), _target.GetComponent<Listener>().automatedNoise);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        DestroyImmediate(_target.GetComponent<Listener>(), true);
                    }
                }
                else
                {
                    if (GUILayout.Button("Add"))
                    {
                        _target.gameObject.AddComponent<Listener>();
                    }
                }
                GUILayout.Space(10f);
                EditorGUILayout.LabelField("Smelling:", EditorStyles.boldLabel);
                if (_target.GetComponent<Smelling>())
                {
                    _target.GetComponent<Smelling>().SmellDistance = EditorGUILayout.FloatField("Smell Distance: ", _target.GetComponent<Smelling>().SmellDistance);
                    if (GUILayout.Button("Remove"))
                    {
                        DestroyImmediate(_target.GetComponent<Smelling>(), true);
                    }
                }
                else
                {
                    if (GUILayout.Button("Add"))
                    {
                        _target.gameObject.AddComponent<Smelling>();
                    }
                }
                GUILayout.Space(10f);
                EditorGUILayout.LabelField("Grid Align:", EditorStyles.boldLabel);
                if (_target.GetComponent<GridAlign>())
                {
                    if (GUILayout.Button("Remove"))
                    {
                        DestroyImmediate(_target.GetComponent<GridAlign>(), true);
                    }
                }
                else
                {
                    if (GUILayout.Button("Add"))
                    {
                        _target.gameObject.AddComponent<GridAlign>();
                    }
                }
            }
        }
    }
}