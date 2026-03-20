using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float backgroundHeight = 10f;

    private Transform[] backgrounds;

    void Start()
    {
        backgrounds = new Transform[3];
        backgrounds[0] = transform.GetChild(0);
        backgrounds[1] = transform.GetChild(1);
        backgrounds[2] = transform.GetChild(2);
    }

    void Update()
    {
        foreach (Transform bg in backgrounds)
        {
            bg.position += Vector3.down * scrollSpeed * Time.deltaTime;
            if (bg.position.y < -backgroundHeight * 1.5f)
            {
                bg.position = new Vector3(bg.position.x,
                                         bg.position.y + backgroundHeight * 3,
                                         bg.position.z);
            }
        }
    }
}
