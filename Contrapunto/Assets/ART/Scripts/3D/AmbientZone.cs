using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    [Header("Audios")]
    public AudioClip ambientClipBad;
    public AudioClip ambientClipGood;
    [Range(0f, 1f)] public float targetVolume = 1f;

    private AudioClip previousClip;
    private float previousVolume;
    private bool goodSoundActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            previousClip = AmbientManager.Instance.GetCurrentClip();
            previousVolume = AmbientManager.Instance.GetCurrentTargetVolume();

            if (!goodSoundActivated && ambientClipBad != null)
            {
                AmbientManager.Instance.ChangeAmbientSound(ambientClipBad, targetVolume);
            }
            else if (goodSoundActivated && ambientClipGood != null)
            {
                AmbientManager.Instance.ChangeAmbientSound(ambientClipGood, targetVolume);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AmbientManager.Instance.ChangeAmbientSound(previousClip, previousVolume);
        }
    }

    public void ActivateGoodAmbient()
    {
        if (!goodSoundActivated && ambientClipGood != null)
        {
            goodSoundActivated = true;
            AmbientManager.Instance.ReplaceCurrentZoneAmbient(ambientClipGood, targetVolume);
        }
    }
}
