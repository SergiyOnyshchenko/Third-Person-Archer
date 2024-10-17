using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class HitFeedbackHelper : MonoBehaviour
{
    [SerializeField] private MMF_Player _player;

    public void Play(int damage)
    {
        _player.PlayFeedbacks(transform.position, damage);
    }
}
