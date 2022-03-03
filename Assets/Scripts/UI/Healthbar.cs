using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    public GameObject HeartPrefab;

    private List<Animator> Hearts = new List<Animator>();
    private int CurrentHealth;
    private Player Player;

    private void Awake()
    {
        Player = FindObjectOfType<Player>();

        CurrentHealth = Player.MaxHealth;
        for(int i = 0; i < Player.MaxHealth; i++)
        {
            GameObject heartObj = Instantiate(HeartPrefab, transform);
            Animator heart = heartObj.GetComponent<Animator>();
            heart.SetBool("Alive", true);
            Hearts.Add(heart);
        }

    }

    private void LateUpdate()
    {
        if(Player.Health != CurrentHealth)
        {
            foreach (Animator heart in Hearts)
                heart.SetBool("Alive", Hearts.IndexOf(heart) >= Player.MaxHealth - Player.Health);

            CurrentHealth = Player.Health;
        }
    }
}
