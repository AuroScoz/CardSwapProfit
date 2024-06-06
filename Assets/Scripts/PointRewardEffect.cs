using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointRewardEffect : MonoBehaviour {
    public static PointRewardEffect Instance;
    [SerializeField] Transform SpawnParent;
    [SerializeField] float MoveSpeed = 500;
    [SerializeField] float DestroyDist = 70;
    [SerializeField] float DontDestryAfterSecs = 1;
    [SerializeField] int MaxSpawnVelocity = 400;
    [SerializeField] float Lerp = 0.001f;
    [SerializeField] GameObject PointEffectPrefab;
    [SerializeField] Transform Target;
    [SerializeField] GameObject GlowEffectPrefab;

    List<Rigidbody2D> Particles = new List<Rigidbody2D>();


    private void Start() {
        Instance = this;
    }
    public void PlayReward(HandType _type) {
        int spawnCount = 0;
        switch (_type) {
            case HandType.HighCard:
                spawnCount = 0;
                break;
            case HandType.Pair:
                spawnCount = 5;
                break;
            case HandType.ThreeOfAKind:
                spawnCount = 10;
                break;
            case HandType.Straight:
                spawnCount = 20;
                break;
            case HandType.Flush:
                spawnCount = 30;
                break;
            case HandType.FullHouse:
                spawnCount = 40;
                break;
            case HandType.FourOfAKind:
                spawnCount = 50;
                break;
            case HandType.StraightFlush:
                spawnCount = 60;
                break;
            default:
                break;
        }
        if (spawnCount > 0) SpanwParticles(spawnCount);
    }

    void SpanwParticles(int _count) {
        for (int i = 0; i < _count; i++) {
            SpanwParticle();
        }
    }
    void SpanwParticle() {
        var go = Instantiate(PointEffectPrefab, SpawnParent);
        Rigidbody2D rigid = go.GetComponent<Rigidbody2D>();
        if (rigid == null) return;
        rigid.velocity = GetRndVelocity();
        Particles.Add(rigid);
        MoveParticles().Forget();
    }
    Vector2 GetRndVelocity() {
        int randForce = Random.Range(MaxSpawnVelocity / 2, MaxSpawnVelocity);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return randForce * direction.normalized;
    }
    async UniTask MoveParticles() {
        bool loop = true;
        // 前幾秒不可以刪除粒子
        bool canDestroy = false;
        UniTask.Void(async () => {
            await UniTask.Delay((int)(DontDestryAfterSecs * 1000f));
            canDestroy = true;
            await UniTask.Delay(5000);
            loop = false;
        });


        while (loop) {
            List<Rigidbody2D> toRemove = new List<Rigidbody2D>();

            if (canDestroy) {
                foreach (var particle in Particles) {
                    if (particle != null) {
                        var posDiff = (Vector2)Target.position - particle.position;
                        if (posDiff.magnitude < DestroyDist) {
                            Destroy(particle.gameObject);
                            toRemove.Add(particle);
                        }
                    }
                }

                foreach (var particle in toRemove) {
                    Particles.Remove(particle);
                }

                if (Particles.Count == 0) {
                    loop = false;
                    break;
                }
            }

            // 粒子移動
            foreach (var particle in Particles) {
                if (particle != null) {
                    Vector2 targetVol = ((Vector2)Target.position - particle.position).normalized * MoveSpeed;
                    particle.velocity = Vector2.Lerp(particle.velocity, targetVol, Lerp);
                }
            }
            await UniTask.Delay(100);
        }

        await UniTask.Delay(100);
    }

}
