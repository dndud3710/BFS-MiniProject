
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    //1. BFS Ž��
    //2. �̵� �� ����
    //3. �Ǽ� ����
    public GameObject foundObject; 
    [SerializeField]
    private float MoveSpeed;


    FindSkill findskill; // �Ǽ��۾��� ã�� Ŭ����
    Animator animator; 

    List<Vector2> Waypoint; // BFS�� ã�� ���
    int WayPointcount; // WayPoint ������� ĳ���Ͱ� �̵��� ��, �ش� ��ǥ�� ��������� ��� ���� ��ǥ�� ����Ű���ϴ� List Index ����

    public Buildings Target => findskill.target; //FindSkill���� ã�� �Ǽ� ������Ʈ�� Buildings Ŭ����
    public enum state // �����̻����� ó��.. ���� �̼���
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
    Vector2 dis_; // WayPoint ĳ�ÿ� 
    Vector3 dis; // WayPoint�� ���� ����

    /// <summary>
    /// 1. BFS�� Ž���� ��� ��������
    /// 2. null�ϰ�� �н�
    /// </summary>
    public void FindWay()
    {
        //�ߺ� ���� ���� (��� �ൿ���� Idle�� �ƴѻ��¿��� ����)
        if (curState != state.Idle) { return; }

        Waypoint = findskill.FindStart();
        if (Waypoint == null)
        {
            //�ֺ��� �Ǽ������Ѱ��� ����
            return;

        }
        //�ִϸ��̼� �� ���� ó��
        foundObject.SetActive(true);
        animator.SetBool("Walking", true);
        animator.SetTrigger("Found");
    }
    /// <summary>
    /// BFS�� Ž���� ��� �ȱ�
    /// </summary>
    private void FindStart()
    {
        //�ִϸ��̼� �� ���� ó��
        foundObject.SetActive(false);
        curState = state.Move;
        animator.SetBool("Walking", true);

        //ù ��η� �ٶ󺸰� �ϱ� ����
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
                //RandomMove(); ����
                break;
            case state.RandomMove:
                /*
                transform.Translate(RandomVec * MoveSpeed* Time.deltaTime,Space.World);
                if (Timer(ref UpdateTimer_Clock, 3f)) {
                    ReturnIdle(curState);
                }*/
                break;
            case state.Move:
                
                //���� ��������Ʈ ���� ���ϱ�
                dis_ = Waypoint[WayPointcount];
                dis = new Vector3(dis_.x, 0, dis_.y) - transform.position;

                //���� ��������Ʈ ����
                if (Vector2.Distance(dis_, new Vector2(transform.position.x, transform.position.z)) < 0.01f)
                {
                    WayPointcount++;
                    //������ ��������Ʈ ���޽�
                    if (WayPointcount >= Waypoint.Count) 
                    {
                        WayPointcount = 0;
                        //�� ����
                        ReturnIdle(curState, WorkStart);
                    }
                    // ��������Ʈ�� ���������� ���� ��� ������ Ȯ�� �� �׹������� ȸ��
                    transform.rotation = Quaternion.LookRotation((new Vector3(Waypoint[WayPointcount].x, 0, Waypoint[WayPointcount].y) - transform.position).normalized);
                }
                //�����ϰ� Translate����Ͽ� ������
                transform.Translate(dis.normalized * MoveSpeed* Time.deltaTime, Space.World);
                break;
            case state.Work:
                
                break;
            case state.Emotion:
                break;
        }
    }

    /// <summary>
    /// ��������Ʈ ���޽� ȣ��Ǵ� �Լ�
    /// �Ǽ� ����
    /// </summary>
    private void WorkStart()
    {
        animator.SetBool("Work", true);
        curState = state.Work;
        //�Ǽ� ���� �ڷ�ƾ ����
        StartCoroutine( GameManager.Instance.BuildTargetStart(Target, WorkEnd));
    }
    /// <summary>
    /// �� ��
    /// </summary>
    private void WorkEnd()
    {
        animator.SetBool("Work", false);
        animator.SetTrigger("EndWork");
    }
    /// <summary>
    /// �⺻ ���� ����
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
    /// ��� Emotion �ൿ���� Animation�� Events�� Ȱ��
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
