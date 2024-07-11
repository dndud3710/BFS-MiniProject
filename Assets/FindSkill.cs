using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Node
{
    public Vector2 curPos;
    public Node ParentNode;
}
//1. float의 비교는 문제가 되어 int로 바꾼뒤 탐색
//2. lossyScale
public class FindSkill : MonoBehaviour
{

    [SerializeField] Vector3 FoundBoxScale = new Vector3(0.5f, 0.1f, 0.5f); // BFS 탐색시 몇 칸의 범위 단위로 탐색할 것인지
    [SerializeField] Vector2 FoundMaxGrid = new Vector2(30f, 30f); //BFS 탐색 범위

    public LayerMask layer;


  
    GameObject WayBlock,WayBlock1, WayBlock2;

    //방문체크 자료구조
    // Dictionary로 한 이유 : 처음에는 List를 써서 Vector 끼리 비교하려 했지만, 소수점의 비교보다, 확실하게 bool값으로 비교하기 위해 사용
    Dictionary<Vector2,bool> visited; 
    //BFS에서 주변 경로를 탐색하기 위해 사용될 Queue
    Queue<Node> WayBlocksQueue; 
    //캐릭터가 이동해야 될 경로
    List<Vector2> WayPoint;

    private int[] xx = { 0 ,1 ,-1 ,0};
    private int[] yy = { 1 ,0 ,0 ,-1};

    int blockCount;
    //건설 오브젝트 컴포넌트
    Buildings target_;
    public Buildings target => target_;
    private void Awake()
    {
        blockCount = 0;
        WayBlock1 = Resources.Load<GameObject>("WayPoint1");
        WayBlock2 = Resources.Load<GameObject>("WayPoint2");

        WayPoint = new List<Vector2>(30);
        visited = new Dictionary<Vector2, bool>(100);

        WayBlocksQueue = new Queue<Node>(200);
    }
    /// <summary>
    /// 다시 탐색하기 전 초기화 과정
    /// </summary>
    void Init()
    {
        WayBlocksQueue.Clear();
        visited.Clear();
        WayPoint.Clear();
        target_ = null;
    }
    /// <summary>
    /// BFS 탐색
    /// </summary>
    /// <returns></returns>
    public List<Vector2> FindStart()
    {
        Init();

        //현재 포지션 방문
        Vector2 pos = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        visited[pos] = true;
        //현재 포지션을 Node로 저장후 Queue삽입
        WayBlocksQueue.Enqueue(new Node() { curPos = pos, ParentNode = null });

        //Queue가 끝날때까지 반복
        //반복이 끝나는 시점은 설정한 탐색범위 바깥이거나, 건설 오브젝트를 탐색했을 때
        while (WayBlocksQueue.Count > 0)
        {
            Node n = WayBlocksQueue.Dequeue();

            for(int i = 0; i < 4; i++)
            {
                
                Vector2 vec = new Vector2(xx[i]+ n.curPos.x, yy[i]+n.curPos.y);

                //방문을 안했던 곳이면 Dictionary에 false상태로 추가
                if(!visited.ContainsKey(vec))
                    visited.Add(vec,false);

                //탐색 범위 바깥
                if (Mathf.Abs( vec.x) > FoundMaxGrid.x || Mathf.Abs(vec.y) > FoundMaxGrid.y)
                {
                    return null;
                }

                //이미 탐색 했을 시
                if (visited[vec] == true) { continue; }

                //탐색 안했을 시
                else
                {
                    //건설오브젝트 탐색
                    Collider[] col = Physics.OverlapBox(new Vector3(vec.x,0f,vec.y),FoundBoxScale/2, Quaternion.identity,layer);
                    if (col.Length >0)
                    {
                        //이미 건설이 완료되었으면 넘어감 (이 때 경로 등록을 하지않기 때문에 오브젝트를 통과하면서 가지않음)
                        target_ = col[0].GetComponent<Buildings>();
                        if (target_.MaxLevelCheck()) continue;
                        return Found(new Node() { curPos = vec, ParentNode = n });
                    }
                    WayBlocksQueue.Enqueue(new Node() { curPos =  vec, ParentNode = n });
                    visited[vec] = true;
                }
            }
        }
        return null;
        
    }
    /// <summary>
    /// Node의 부모를 따라가 경로를 재탐색
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    List<Vector2> Found(Node node)
    {
        while (node != null)
        {
            Vector2 vec = new Vector2(node.curPos.x, node.curPos.y);
            WayPoint.Add(vec);
            node = node.ParentNode;
        }
        WayPoint.Reverse();


        //방향을 비교하여 경로가 바뀔때만 List에 경로를 삽입하여 모든 경로를 탐색하지않게 함
        List<Vector2> list = new List<Vector2>();
        Vector2 prevVec = Vector2.zero;
        Vector2 PrevNormal = Vector2.zero;
        foreach(var item in WayPoint)
        {
            if (prevVec != Vector2.zero)
            {
                if ((item - prevVec).normalized != PrevNormal)
                {
                    list.Add(prevVec);
                }
            }
            else
            {
                list.Add(item);
            }
            
            PrevNormal = (item - prevVec).normalized;
            prevVec = item;
        }
        list.Add(WayPoint[WayPoint.Count - 1]);

        foreach (var d in list)
        {
            Debug.Log(d);
        }
        StartCoroutine(BlockCoroutine());
        return list;
    }
    //BFS로 제일 가까운 물체 찾은후 Node그래프를 따라 경로 시각적 표현하기
     IEnumerator  BlockCoroutine()
    {
        foreach(var item in WayPoint)
        {
            MakeBlock(item);
            yield return new WaitForSeconds(0.04f);
        }
    }

    private Transform MakeBlock(Vector2 pos)
    {
        blockCount++;
        GameObject g_ = WayBlock1;
        if ((blockCount % 2) == 0)
        {
            g_ = WayBlock2;
        }

        Vector3 pos_ = new Vector3(pos.x, 0, pos.y);
        GameObject g = Instantiate(g_, pos_, Quaternion.identity);
        Destroy(g, 2f);
        return g.transform;
    }
}
