using UnityEngine;

public class LandReciever : MonoBehaviour
{
    // Assign your footstep sounds here in the Inspector
    public AudioClip[] jumpLandingClips;

    // An AudioSource to play the sounds
    public AudioSource audioSource;

    // Called by the Animation Event named "OnLand"
    public void OnLand()
    {
        if (jumpLandingClips.Length == 0) return;

        // Pick a random clip index
        int randIndex = Random.Range(0, jumpLandingClips.Length);
        AudioClip clip = jumpLandingClips[randIndex];

        // Play the chosen landing sound
        audioSource.pitch = 1.0f + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1.0f; // reset if you want

        // Optional: add random pitch variation
        // audioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);
    }
}
