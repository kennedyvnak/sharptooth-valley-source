using NFHGame.AudioManagement;
using NFHGame.Battle;
using NFHGame.Characters;
using NFHGame.RangedValues;
using UnityEngine;

namespace NFHGame {
    public class RockProvider : ScriptableSingletons.ScriptableSingleton<RockProvider> {
        [System.Serializable]
        public class Rock {
            public Sprite sprite;
            public Rect colliderSize;
            public Sprite[] breakFrames;
            public AudioProviderObject breakSound;
        }

        [SerializeField] private FallenRock m_RockPrefab;
        [SerializeField] private float m_SpawnRangeX;
        [SerializeField, RangedValue(-100.0f, 100.0f)] private RangedFloat m_SpawnRangeY;
        [SerializeField, RangedValue(0.0f, 1.0f)] private RangedFloat m_RockChances;

        [SerializeField] private RangedFloat[] m_FakeRocksTargetY;
        [SerializeField] private float m_FakeRockChance;
        [SerializeField] private float m_ForegroundStarts;

        [Space]
        [SerializeField] private Rock[] m_SmallRocks;
        [SerializeField] private Rock[] m_MediumRocks;
        [SerializeField] private Rock[] m_LargeRocks;

        public Rock[] smallRocks => m_SmallRocks;
        public Rock[] mediumRocks => m_MediumRocks;
        public Rock[] largeRocks => m_LargeRocks;

        public float fakeRockChance { get => m_FakeRockChance; set => m_FakeRockChance = value; }

        public FallenRock SpawnRandomRock(Transform parent) => SpawnRandomRock(GetRandomSize(), GetRandomPosition(), parent);
        public FallenRock SpawnRandomRock(Vector2 position, Transform parent) => SpawnRandomRock(GetRandomSize(), position, parent);
        public FallenRock SpawnRandomRock(FallenRock.RockSize size, Transform parent) => SpawnRandomRock(size, GetRandomPosition(), parent);
        public FallenRock SpawnRandomRock(FallenRock.RockSize size, Vector2 position, Transform parent) => SpawnRock(GetRandomRock(size), size, position, parent);
        public FallenRock SpawnRock(Rock rock, FallenRock.RockSize size, Vector2 position, Transform parent) => SpawnRock(rock, size, position, Random.value > m_FakeRockChance, parent);


        public FallenRock SpawnRock(Rock rock, FallenRock.RockSize size, Vector2 position, bool fakeRock, Transform parent) {
            var rockInstance = Instantiate(m_RockPrefab, position, Quaternion.identity, parent);
            rockInstance.Setup(size, rock.colliderSize, rock.sprite, rock.breakFrames, rock.breakSound);
            if (fakeRock) {
                rockInstance.fakeRock = true;
                rockInstance.targetY = m_FakeRocksTargetY[Random.Range(0, m_FakeRocksTargetY.Length)].RandomRange();
                rockInstance.SetIsForeground(rockInstance.targetY < m_ForegroundStarts);
            }
            return rockInstance;
        }

        public Vector2 GetRandomPosition() {
            var bastPosX = GameCharactersManager.instance.bastheet.rb.position.x;
            return new Vector2(Random.Range(bastPosX - m_SpawnRangeX, bastPosX + m_SpawnRangeX), m_SpawnRangeY.RandomRange());
        }

        public FallenRock.RockSize GetRandomSize() {
            var rng = Random.value;
            if (rng < m_RockChances.min) return FallenRock.RockSize.Small;
            else if (rng < m_RockChances.max) return FallenRock.RockSize.Medium;
            else return FallenRock.RockSize.Large;
        }

        public Rock GetRandomRock(FallenRock.RockSize size) {
            return size switch {
                FallenRock.RockSize.Small => smallRocks[Random.Range(0, smallRocks.Length)],
                FallenRock.RockSize.Medium => mediumRocks[Random.Range(0, mediumRocks.Length)],
                FallenRock.RockSize.Large => largeRocks[Random.Range(0, largeRocks.Length)],
                _ => null
            };
        }
    }
}
