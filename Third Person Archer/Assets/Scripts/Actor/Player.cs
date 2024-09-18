using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class Player : ActorController
    {
        public static Player Instance;

        protected override void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            base.Awake();
        }
    }
}
