using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class School : MonoBehaviour
{
    protected float HP;
    private float MaxHP;
    public float initialHP;
    protected static School instance = null;
    public static int stack; // nn번째 격파에 사용
    private static bool IsBoss;
    private bool isShake;
    private int bossType;
    private int BossMoney;
    Coroutine shake;
    Rigidbody2D rb;
    SpriteRenderer sr;
    BoxCollider2D boxCollider;
    AudioSource audioSource;
    BossBubbleSpawn[] bossbubbleSpawn = new BossBubbleSpawn[2];

    [SerializeField] private int bossPeriod;

    [Header("UI")]
    [SerializeField] private Image SchoolHPBar;
    [SerializeField] private TextMeshProUGUI SchoolHPText;
    [SerializeField] private TextMeshProUGUI SchoolNameText;
    [SerializeField] private GameObject BossAlertLine;
    [SerializeField] private TextMeshProUGUI RewardText;
    [SerializeField] private GameObject[] BubbleCanvas;

    [Header("Speed")]
    [SerializeField] private float SchoolSpeed;
    [SerializeField] private float BossSpeed;
    private float currentSpeed;

    [Header("Shake")]
    [SerializeField] private float shakeTime;
    [SerializeField] private float shakeIntensity;

    [Header("BreakStage")]
    [SerializeField] private Sprite[] BreakStages;
    [SerializeField] private BossData[] bossData;

    void Awake()
    {
        if (null == instance)
        {
            //이 클래스 인스턴스가 탄생했을 때 전역변수 instance에 게임매니저 인스턴스가 담겨있지 않다면, 자신을 넣어준다.
            instance = this;
            IsBoss = false;
            isShake = false;
            School.stack = 0;
            MaxHP = initialHP;

            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
            audioSource = GetComponent<AudioSource>();
            bossbubbleSpawn[0] = transform.GetChild(1).gameObject.GetComponent<BossBubbleSpawn>();
            bossbubbleSpawn[1] = transform.GetChild(2).gameObject.GetComponent<BossBubbleSpawn>();

            shake = null;
            //씬 전환이 되더라도 파괴되지 않게 한다.
            //gameObject만으로도 이 스크립트가 컴포넌트로서 붙어있는 Hierarchy상의 게임오브젝트라는 뜻이지만, 
            //나는 헷갈림 방지를 위해 this를 붙여주기도 한다.
            // DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //만약 씬 이동이 되었는데 그 씬에도 Hierarchy에 GameMgr이 존재할 수도 있다.
            //그럴 경우엔 이전 씬에서 사용하던 인스턴스를 계속 사용해주는 경우가 많은 것 같다.
            //그래서 이미 전역변수인 instance에 인스턴스가 존재한다면 자신(새로운 씬의 GameMgr)을 삭제해준다.
            Destroy(this.gameObject);
        }

        // Resolution
        if (Screen.width < Screen.height)
        {
            float width = Camera.main.orthographicSize * 1.88f * Screen.width / Screen.height / 10.0f;
            transform.localScale = new Vector3(width, width, width);
        }
        BossMoney = 10;
    }

    public static School getInstance()
    {
        return instance;
    }

    protected void Start()
    {
        ReGen();
    }

    private void OnEnable()
    {
        if (IsBoss)
        {
            bossbubbleSpawn[0].StartSpawnBubble(bossData[bossType]);
            bossbubbleSpawn[1].StartSpawnBubble(bossData[bossType]);
        }
    }

    private void Update()
    {
        if (Mathf.Abs(transform.position.y) >= 7.3f)
        {
            transform.position = new Vector3(0, 7.3f);
            rb.velocity = Vector3.zero;
        }
    }

    public void ReGen()
    {
        // Stop Coroutines
        if (isShake)
        {
            StopCoroutine(shake);
            isShake = false;;
        }
        if (IsBoss)
        {
            bossbubbleSpawn[0].StopSpawnBubble();
            bossbubbleSpawn[1].StopSpawnBubble();
            IsBoss = false;
        }

        stack++;
        SchoolHPBar.fillAmount = 1;
        rb.velocity = Vector2.zero;
        transform.position = new Vector3(0, 7.3f);

        // bossPeriod번째마다 보스 체크
        if (stack % bossPeriod == 0)
        {
            BossAlertLine.SetActive(true);
            currentSpeed = BossSpeed;
            HP = MaxHP * 5;
            IsBoss = true;
            bossType = Random.Range(0, bossData.Length);
            SchoolNameText.text = bossData[bossType].Name + " 학교";
            sr.sprite = bossData[bossType].Stages[0];
        }
        else
        {
            gameObject.SetActive(true);
            currentSpeed = SchoolSpeed;
            SchoolNameText.text = stack + "번째 학교";
            HP = MaxHP;
            sr.sprite = BreakStages[0];
        }

        SchoolHPText.text = (int)HP + "/" + (int)HP;
        rb.gravityScale = 0.35f * currentSpeed;
    }

    public void Reset()
    {
        stack = 0;
        MaxHP = initialHP;
        SchoolSpeed /= BossSpeed;
        BossSpeed = 1;
        
        if (isShake)
        {
            StopCoroutine(shake);
            isShake = false;
        }

        ReGen();
    }

    private void NextPhase()
    {
        MaxHP *= 1.15f;          // HP +1%
        SchoolSpeed += 0.03f;   // speed +0.03
        BossSpeed += 0.03f;     // speed +0.03

        Money.IncreaseMoney(BossMoney);
        BossMoney = (int) (BossMoney * 1.2f);
        RewardText.text = BossMoney.ToString("+#,##0");
    }

    private void GetAttack(float dmg)
    {
        HP -= dmg;
        if (IsBoss)
        {
            SchoolHPBar.fillAmount = HP / (MaxHP * 5);
            SchoolHPText.text = (int)Mathf.Max(HP, 0) + "/" + (int)(MaxHP * 5);
        }
        else
        {
            SchoolHPBar.fillAmount = HP / MaxHP;
            SchoolHPText.text = (int)Mathf.Max(HP, 0) + "/" + (int)MaxHP;
        }

        // Change Sprite
        if (HP <= 0)
        {
            Dead();
            return;
        }
        float unit = 1f / BreakStages.Length;
        int stage = (int)(SchoolHPBar.fillAmount / unit);
        if (IsBoss)
        {
            sr.sprite = bossData[bossType].Stages[BreakStages.Length - stage - 1];
        }
        else
        {
            sr.sprite = BreakStages[BreakStages.Length - stage - 1];
        }
        boxCollider.size = sr.sprite.bounds.size;
    }

    public void GetAttackByPlayer(float dmg)
    {
        GetAttack(dmg);
        //if(!WindSkill.usingWind)
            rb.velocity = rb.velocity.y>0f?rb.velocity:Vector3.zero;

        if (gameObject.activeSelf)
        {
            shake = StartCoroutine(Shake());
        }
    }

    public void GetAttackByWind(float dmg)
    {
        rb.AddForce(Vector2.up * 3, ForceMode2D.Impulse);
        audioSource.Play();
        GetAttack(dmg);
    }

    public void GetAttackByStudent(float dmg)
    {
        GetAttack(dmg);
    }

    private IEnumerator Shake()
    {
        Vector3 startPosition = transform.position;
        float time = shakeTime;

        while (time > 0.0f)
        {
            time -= Time.unscaledDeltaTime;
            transform.position = startPosition + Random.insideUnitSphere * shakeIntensity;

            yield return null;
        }

        transform.position = new Vector3(0, startPosition.y - currentSpeed * shakeTime, 0);
    }

    private void Dead()
    {
        // 대충 죽는 처리
        if (isShake)
        {
            StopCoroutine(shake);
            isShake = false;
        }
        if (IsBoss)
        {
            bossbubbleSpawn[0].StopSpawnBubble();
            bossbubbleSpawn[1].StopSpawnBubble();
            GameManager.Instance.DestroyBossBubbles();
        }

        gameObject.SetActive(false);
        if (IsBoss)
        {
            NextPhase();
            GameManager.Instance.BossClear();
            IsBoss = false;
        }
        
        ReGen();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "GameOver")
        {
            if (isShake)
            {
                StopCoroutine(shake);
                shake = null;
            }
            GameManager.Instance.GameOver();
        }
    }
}
