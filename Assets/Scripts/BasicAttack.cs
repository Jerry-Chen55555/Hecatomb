using System;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    public GameObject SlashPrefab;
    public GameObject Player;
    public float SlashCooldown = 0.3f;
    public float SlashOffset = 1f;
    public Camera cameraMain;

    private float _nextSlashTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= _nextSlashTime)
        {
            Slash();
            _nextSlashTime = Time.time + SlashCooldown;
        }
    }

    void Slash()
    {
        Vector3 mousePosition = cameraMain.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        
        Vector2 direction = (mousePosition - transform.position).normalized;
        Vector3 spawnPosition = transform.position + (Vector3)(direction * SlashOffset);

        float angle = Mathf.Atan2(direction.y, direction.x) * (180f/Mathf.PI);

        Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90);

        GameObject slash = Instantiate(SlashPrefab, spawnPosition, rotation);
        slash.transform.SetParent(Player.transform);
    }
}