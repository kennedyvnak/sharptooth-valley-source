using NFHGame.ScriptableSingletons;
using UnityEngine;

namespace NFHGame.Battle {
    public class BattleProvider : ScriptableSingleton<BattleProvider> {
        [System.Serializable]
        public class FallenRock {
            public float gravityScale;
            public float maxFallSpeed;

            public float shadowAlpha;
            public float shadowHeightRatio;
            public float shadowStartY;
            public AnimationCurve shadowAlphaCurve;
            public AnimationCurve shadowScaleCurve;
            public Vector2 shadowOffset;
            public LayerMask groundLayer, characterLayer;
            public float groundRaycastDistance;
            public float rockFadeDuration;
            public float rockFadeDelay;

            public int mediumRockGroundCrack;
            public int largeRockGroundCrack;

            public float bastheetHeadHeight;
            public float dinnerHeadHeight;

            public RaycastHit2D RaycastGround(Vector2 position) {
                return Physics2D.Raycast(position, Vector2.down, groundRaycastDistance, groundLayer);
            }
        }

        [System.Serializable]
        public class FloatingGround {
            public int defaultGroundLife;
            public float regenerationDuration;
            public Sprite[] groundSpritesByLife;
        }

        [System.Serializable]
        public class BastheetStunTimes {
            public float littleStun;
            public float hightStun;
        }

        [System.Serializable]
        public class ForceField {
            public int mediumRockReduction;
            public int largeRockReduction;
            public int plasmaAbsorbForce;
            public int miniLaserAbsorbForce;
            public int maxForce;
        }

        [System.Serializable]
        public class Characters {
            public float dinnerOffset;
            public float dinnerKnockoutUpTime;

            public float dinnerStunLowTime;
            public float dinnerStunMediumTime;

            public float dinnerAccelerationDuration;
            public float dinnerAccelerationCooldown;
            public AnimationCurve dinnerAccelerationCurve;
        }


        [SerializeField] private FallenRock m_FallenRock;
        [SerializeField] private FloatingGround m_FloatingGround;
        [SerializeField] private BastheetStunTimes m_StunTimes;
        [SerializeField] private ForceField m_ForceField;
        [SerializeField] private Characters m_Characters;

        public FallenRock fallenRock => m_FallenRock;
        public FloatingGround floatingGround => m_FloatingGround;
        public BastheetStunTimes stunTimes => m_StunTimes;
        public ForceField forceField => m_ForceField;
        public Characters characters => m_Characters;
    }
}