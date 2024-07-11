
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    //1. BFS 탐색
    //2. 이동 후 도착
    //3. 건설 시작
    public GameObject foundObject; 
    [SerializeField]
    private float MoveSpeed;


    FindSkill findskill; // 건설작업을 찾는 클래스
    Animator animator; 

    List<Vector2> Waypoint; // BFS로 찾은 경로
    int WayPointcount; // WayPoint 순서대로 캐릭터가 이동할 때, 해당 좌표에 가까워졌을 경우 다음 좌표를 가르키게하는 List Index 변수

    public Buildings Target => findskill.target; //FindSkill에서 찾은 건설 오브젝트의 Buildings 클래스
    public enum state // 상태이상으로 처리.. 아직 미숙함
    {
        Idle,
        RandomMove,
        Move,
        Work,
        Emotion
    }

    private state curState;
    private void Awake()
    {
        curState = state.Idle;
        animator = GetComponent<Animator>();
        findskill = GetComponent<FindSkill>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    Vector2 dis_; // WayPoint 캐시용 
    Vector3 dis; // WayPoint로 가는 방향

    /// <summary>
    /// 1. BFS로 탐색한 경로 가져오기
    /// 2. null일경우 패스
    /// </summary>
    public void FindWay()
    {
        //중복 실행 방지 (모든 행동들은 Idle이 아닌상태에서 진행)
        if (curState != state.Idle) { return; }

        Waypoint = findskill.FindStart();
        if (Waypoint == null)
        {
            //주변에 건설가능한것이 없음
            return;

        }
        //애니메이션 및 상태 처리
        foundObject.SetActive(true);
        animator.SetBool("Walking", true);
        animator.SetTrigger("Found");
    }
    /// <summary>
    /// BFS로 탐색한 경로 걷기
    /// </summary>
    private void FindStart()
    {
        //애니메이션 및 상태 처리
        foundObject.SetActive(false);
        curState = state.Move;
        animator.SetBool("Walking", true);

        //첫 경로로 바라보게 하기 위함
        dis_ = Waypoint[WayPointcount];
        dis = new Vector3(dis_.x, 0, dis_.y) - transform.position;
        transform.rotation = Quaternion.LookRotation((new Vector3(Waypoint[WayPointcount].x, 0, Waypoint[WayPointcount].y) - transform.position).normalized);
    }
    // Update is called once per frame

    Vector3 RandomVec;
    float UpdateTimer_Clock;

    void Update()
    {
        switch (curState)
        {
            case state.Idle:
                //RandomMove(); 무시
                break;
            case state.RandomMove:
                /*
                transform.Translate(RandomVec * MoveSpeed* Time.deltaTime,Space.World);
                if (Timer(ref UpdateTimer_Clock, 3f)) {
                    ReturnIdle(curState);
                }*/
                break;
            case state.Move:
                
                //다음 웨이포인트 방향 구하기
                dis_ = Waypoint[WayPointcount];
                dis = new Vector3(dis_.x, 0, dis_.y) - transform.position;

                //다음 웨이포인트 도달
                if (Vector2.Distance(dis_, new Vector2(transform.position.x, transform.position.z)) < 0.01f)
                {
                    WayPointcount++;
                    //마지막 웨이포인트 도달시
                    if (WayPointcount >= Waypoint.Count) 
                    {
                        WayPointcount = 0;
                        //일 시작
                        ReturnIdle(curState, WorkStart);
                    }
                    // 웨이포인트에 도달했을때 다음 경로 방향을 확인 후 그방향으로 회전
                    transform.rotation = Quaternion.LookRotation((new Vector3(Waypoint[WayPointcount].x, 0, Waypoint[WayPointcount].y) - transform.position).normalized);
                }
                //간단하게 Translate사용하여 움직임
                transform.Translate(dis.normalized * MoveSpeed* Time.deltaTime, Space.World);
                break;
            case state.Work:
                
                break;
            case state.Emotion:
                break;
        }
    }

    /// <summary>
    /// 웨이포인트 도달시 호출되는 함수
    /// 건설 시작
    /// </summary>
    private void WorkStart()
    {
        animator.SetBool("Work", true);
        curState = state.Work;
        //건설 로직 코루틴 실행
        StartCoroutine( GameManager.Instance.BuildTargetStart(Target, WorkEnd));
    }
    /// <summary>
    /// 일 끝
    /// </summary>
    private void WorkEnd()
    {
        animator.SetBool("Work", false);
        animator.SetTrigger("EndWork");
    }
    /// <summary>
    /// 기본 상태 변경
    /// </summary>
    /// <param name="PrevState"></param>
    /// <param name="Callback"></param>
    private void ReturnIdle(state PrevState,Action Callback=null)
    {
        if(PrevState == state.RandomMove || PrevState == state.Move)
            animator.SetBool("Walking", false);
        curState = state.Idle;
        Callback?.Invoke();
    }

    float RandomMove_Time =0 ;
    //private void RandomMove()
    //{
    //    if (Timer(ref RandomMove_Time, 3f))
    //    {
    //        RandomVec =  new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
    //        transform.rotation = Quaternion.LookRotation(RandomVec.normalized);
    //        animator.SetBool("Walking", true);
    //        curState = state.RandomMove;
    //    }
    //}
    private bool Timer(ref float Clock,float time)
    {
        Clock += Time.deltaTime;
        if (Clock > time)
        {
            Clock = 0f;
            return true;
        }
        return false;
    }


    /// <summary>
    /// 모든 Emotion 행동들은 Animation의 Events를 활용
    /// </summary>
    public void OnEndFoundEmotion()
    {
        FindStart();
    }
    public void OnEndWorkEmotion()
    {
        ReturnIdle(curState);
    }
}
