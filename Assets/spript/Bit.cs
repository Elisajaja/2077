using UnityEngine;

public class Bit : MonoBehaviour
{

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time % 2 < 1)
        {
            transform.Translate(Vector3.up * Time.deltaTime * 5);
        }
        else
        {
            transform.Translate(Vector3.down * Time.deltaTime * 5);

        }
    }
}
