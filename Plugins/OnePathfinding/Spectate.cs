using UnityEngine;

public class Spectate : MonoBehaviour
{
    public GameObject target;
    private GameObject[] AIs;
    private int cur;

    private void OnGUI()
    {
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 25, 200, 25), "Health: " + target.GetComponent<AIData>().Health.ToString() + "/100");
        GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height - 25, 200, 25), "Hunger: " + target.GetComponent<AIData>().Hunger.ToString() + "/250");
        GUI.Label(new Rect(Screen.width / 2 + 100, Screen.height - 25, 200, 25), "State: " + target.GetComponent<AdvancedAI>().AIState.ToString());

        GUI.Label(new Rect(Screen.width / 2 - 100, 0, 200, 25), "Current: " + cur + "/" + AIs.Length);
    }

    private void Start()
    {
        AIs = GameObject.FindGameObjectsWithTag("AI");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AIs = GameObject.FindGameObjectsWithTag("AI");
            cur++;
            if (cur >= AIs.Length)
            {
                cur = 0;
            }
            target = AIs[cur];
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AIs = GameObject.FindGameObjectsWithTag("AI");
            cur--;
            if (cur < 0)
            {
                cur = AIs.Length - 1;
            }
            target = AIs[cur];
        }
        if (target == null)
        {
            AIs = GameObject.FindGameObjectsWithTag("AI");
            target = AIs[cur];
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