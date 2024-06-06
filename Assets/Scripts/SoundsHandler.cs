using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsHandler : MonoBehaviour
{
    private bool _isOn = true;
    [SerializeField] private AudioSource _audioSource;

    public void ButtonClicked()
    {
        if (_isOn)
        {
            _isOn = false;
            _audioSource.mute = true;
        }
        else
        {
            _isOn = true;
            _audioSource.mute = false;
        }
    }
}
