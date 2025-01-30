using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class cooinn5 : MonoBehaviour
{
    public Text coinText;
    public AudioSource coinSound;
    private int coinsCount = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other. CompareTag("Coins"))
        {
            coinsCount++;
           coinText.text = "All coins: " + coinsCount.ToString();
            coinSound.Play();
            Destroy(other.gameObject);
        }
    }    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
