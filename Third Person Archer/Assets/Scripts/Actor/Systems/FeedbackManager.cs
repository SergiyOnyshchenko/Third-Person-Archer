using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.Feedbacks;

namespace Actor
{
    public class FeedbackManager : System
    {
        [Serializable]
        public class FeedbackInstance
        {
            [field: SerializeField] public string Name {  get; private set; }
            [field: SerializeField] public MMF_Player Feedback { get; private set; }
            [field: SerializeField] public bool IsEnabled { get; private set; } 

            public bool TryPlay(string feedbackName)
            {
                if (!IsEnabled || Name != feedbackName)
                    return false;

                if (Feedback == null)
                {
                    Debug.LogWarning($"Feedback '{Name}' is not assigned in FeedbackManager.");
                    return false;
                }

                Feedback.PlayFeedbacks();
                return true;
            }

            public void SetEnabled(bool enabled)
            {
                IsEnabled = enabled;
            }
        }

        [field: SerializeField] public FeedbackInstance[] Feedbacks {  get; private set; }
        [SerializeField] private bool _areAllFeedbacksEnabled = true;

        public void PlayFeedback(string feedbackName)
        {
            if (!_areAllFeedbacksEnabled)
            {
                return;
            }

            bool played = false;
            foreach (var feedback in Feedbacks)
            {
                if (feedback.TryPlay(feedbackName))
                {
                    played = true;
                    break;
                }
            }

            if (!played)
            {
                
            }
        }

        // Enable or disable all feedbacks
        public void SetAllFeedbacksEnabled(bool enabled)
        {
            _areAllFeedbacksEnabled = enabled;
        }

        // Enable or disable a specific feedback by name
        public void SetFeedbackEnabled(string feedbackName, bool enabled)
        {
            bool found = false;
            foreach (var feedback in Feedbacks)
            {
                if (feedback.Name == feedbackName)
                {
                    feedback.SetEnabled(enabled);
                    found = true;
                }
            }

            if (!found)
            {

            }
        }
    }
}
