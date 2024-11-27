using UnityEngine;

public class ShadowFollow : MonoBehaviour
{
    public Transform character; // Reference to the character
    public float shadowOffset = 0.01f; // Slight offset to avoid z-fighting
    public LayerMask groundLayer; // LayerMask to define what counts as "ground"
    private Material shadowMaterialInstance; // Instance of the shadow's material
    public Material shadowMaterial; // Reference to the original material
    public float maxDistance = 10f; // Max distance for the shadow to be fully invisible

    void Start()
    {
        
        // Create an instance of the material so we don't modify the shared material
        if (shadowMaterial != null)
        {
            shadowMaterialInstance = new Material(shadowMaterial);
            GetComponent<Renderer>().material = shadowMaterialInstance;
        }
    }

    void Update()
    {
        if (character != null && shadowMaterialInstance != null)
        {
            // Raycast downward to find the ground, ignoring layers that are not "Ground"
            Ray ray = new Ray(character.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                // Place the shadow on the ground at the hit point
                transform.position = hit.point + Vector3.up * shadowOffset;

                // Set rotation so the shadow faces upward (XZ plane)
                transform.rotation = Quaternion.Euler(90, 0, 0); // Always point up

                // Adjust opacity based on height
                float distance = hit.distance; // Distance from character to ground
                float alpha = Mathf.Clamp01(1 - (distance / maxDistance)); // Fade out with distance
                
                // Update shadow material's alpha float
                shadowMaterialInstance.SetFloat("_BaseColor", alpha);

                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90, 0, 0);
            }
            else
            {
                // If no "Ground" layer is hit, make the shadow fully transparent
                shadowMaterialInstance.SetFloat("_BaseColor", 0);
            }
        }
    }
}



// transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(90, 0, 0);
