using NFHGame.RangedValues;
using System.Collections.Generic;
using UnityEngine;

namespace NFHGame.Battle {
    public class RockSpawner : MonoBehaviour {
        [SerializeField] private float m_RockBlockPosTime;
        [SerializeField] private float m_RockBlockPosRange;
        [SerializeField] private int m_RockSpawnCount;
        [SerializeField] private RangedFloat m_GroundRange;
        public bool onlyFakeRocks { get; set; }

        private List<Vector2> _rockBlock = new List<Vector2>(); // x: position y: time 

        public void SpawnRandomRock() {
            var rockProvider = RockProvider.instance;
            var size = rockProvider.GetRandomSize();
            bool fakeRock = Random.value < rockProvider.fakeRockChance;
            var rock = rockProvider.SpawnRock(rockProvider.GetRandomRock(size), size, GetPosition(rockProvider, fakeRock), !fakeRock ? onlyFakeRocks : fakeRock, transform);
            rock.fakeBroke = m_GroundRange.Contains(rock.targetY);
        }

        public void EndBattle() {
            foreach (Transform child in transform) {
                if (child.TryGetComponent<FallenRock>(out var rock)) {
                    rock.fakeBroke = m_GroundRange.Contains(rock.targetY);
                    rock.fakeRock = true;
                    rock.overrideBreak = 0;
                }
            }
        }

        private Vector2 GetPosition(RockProvider rockProvider, bool fakeRock) {
            if (fakeRock)
                return rockProvider.GetRandomPosition();

            int spawm = 0;
            bool clear = false;

            float time = Time.time;

            while (spawm < m_RockSpawnCount) {
                Vector2 pos = rockProvider.GetRandomPosition();
                float t = pos.x;

                bool blocked = false;
                foreach (var block in _rockBlock) {
                    if (time - block.y >= m_RockBlockPosTime) {
                        clear = true;
                    } else if (t >= block.x - m_RockBlockPosRange && t <= block.x + m_RockBlockPosRange) {
                        blocked = true;
                        break;
                    }
                }
                if (!blocked) {
                    _rockBlock.Add(new Vector2(pos.x, time));
                    return pos;
                }
                spawm++;
            }

            if (clear) {
                _rockBlock.RemoveAll(x => time - x.y >= m_RockBlockPosTime);
            }

            Vector2 rng = rockProvider.GetRandomPosition();
            _rockBlock.Add(new Vector2(rng.x, time));
            return rng;
        }
    }
}
