using UnityEngine;

/// <summary>
/// This file is used for spectating the objects in the scene, this is only for testing purposes.
/// </summary>
public class Spectate : MonoBehaviour
{
    /// <summary>
    /// The currently active target.
    /// </summary>
    public GameObject target;

    /// <summary>
    /// A list of all AIs in the scene.
    /// </summary>
    private AdvancedAI[] AIs;

    /// <summary>
    /// The current index in the AIs array
    /// </summary>
    private int cur;

    /// <summary>
    /// Used to draw GUI
    /// </summary>
    private void OnGUI()
    {
        if (target != null)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 25, 200, 25), "Health: " + target.GetComponent<AIData>().Health.ToString() + "/100");
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height - 25, 200, 25), "Hunger: " + target.GetComponent<AIData>().Hunger.ToString() + "/250");
            GUI.Label(new Rect(Screen.width / 2 + 100, Screen.height - 25, 200, 25), "State: " + target.GetComponent<AdvancedAI>().AIState.ToString());

            GUI.Label(new Rect(Screen.width / 2 - 100, 0, 200, 25), "Current: " + cur + "/" + AIs.Length);
        }
    }

    /// <summary>
    /// Called on start of component
    /// </summary>
    private void Start()
    {
        AIs = FindObjectsOfType<AdvancedAI>();
    }

    /// <summary>
    /// Called once a frame.
    /// </summary>
    private void Update()
    {
        AIs = FindObjectsOfType<AdvancedAI>();
        if (AIs.Length != 0)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                AIs = FindObjectsOfType<AdvancedAI>();
                cur++;
                if (cur >= AIs.Length)
                {
                    cur = 0;
                }
                target = AIs[cur].gameObject;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                AIs = GameObject.FindObjectsOfType<AdvancedAI>();
                cur--;
                if (cur < 0)
                {
                    cur = AIs.Length - 1;
                }
                target = AIs[cur].gameObject;
            }
            if (target == null)
            {
                AIs = FindObjectsOfType<AdvancedAI>();
                target = AIs[cur].gameObject;
            }

            if (target != null)
            {
                transform.SetParent(target.transform);
                transform.localPosition = new Vector3(0, 3, -3);
                transform.localEulerAngles = new Vector3(30, 0, 0);
                transform.localScale = Vector3.one;
            }
        }
    }
}