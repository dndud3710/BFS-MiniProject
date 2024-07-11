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
//1. float�� �񱳴� ������ �Ǿ� int�� �ٲ۵� Ž��
//2. lossyScale
public class FindSkill : MonoBehaviour
{

    [SerializeField] Vector3 FoundBoxScale = new Vector3(0.5f, 0.1f, 0.5f); // BFS Ž���� �� ĭ�� ���� ������ Ž���� ������
    [SerializeField] Vector2 FoundMaxGrid = new Vector2(30f, 30f); //BFS Ž�� ����

    public LayerMask layer;


  
    GameObject WayBlock,WayBlock1, WayBlock2;

    //�湮üũ �ڷᱸ��
    // Dictionary�� �� ���� : ó������ List�� �Ἥ Vector ���� ���Ϸ� ������, �Ҽ����� �񱳺���, Ȯ���ϰ� bool������ ���ϱ� ���� ���
    Dictionary<Vector2,bool> visited; 
    //BFS���� �ֺ� ��θ� Ž���ϱ� ���� ���� Queue
    Queue<Node> WayBlocksQueue; 
    //ĳ���Ͱ� �̵��ؾ� �� ���
    List<Vector2> WayPoint;

    private int[] xx = { 0 ,1 ,-1 ,0};
    private int[] yy = { 1 ,0 ,0 ,-1};

    int blockCount;
    //�Ǽ� ������Ʈ ������Ʈ
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
    /// �ٽ� Ž���ϱ� �� �ʱ�ȭ ����
    /// </summary>
    void Init()
    {
        WayBlocksQueue.Clear();
        visited.Clear();
        WayPoint.Clear();
        target_ = null;
    }
    /// <summary>
    /// BFS Ž��
    /// </summary>
    /// <returns></returns>
    public List<Vector2> FindStart()
    {
        Init();

        //���� ������ �湮
        Vector2 pos = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        visited[pos] = true;
        //���� �������� Node�� ������ Queue����
        WayBlocksQueue.Enqueue(new Node() { curPos = pos, ParentNode = null });

        //Queue�� ���������� �ݺ�
        //�ݺ��� ������ ������ ������ Ž������ �ٱ��̰ų�, �Ǽ� ������Ʈ�� Ž������ ��
        while (WayBlocksQueue.Count > 0)
        {
            Node n = WayBlocksQueue.Dequeue();

            for(int i = 0; i < 4; i++)
            {
                
                Vector2 vec = new Vector2(xx[i]+ n.curPos.x, yy[i]+n.curPos.y);

                //�湮�� ���ߴ� ���̸� Dictionary�� false���·� �߰�
                if(!visited.ContainsKey(vec))
                    visited.Add(vec,false);

                //Ž�� ���� �ٱ�
                if (Mathf.Abs( vec.x) > FoundMaxGrid.x || Mathf.Abs(vec.y) > FoundMaxGrid.y)
                {
                    return null;
                }

                //�̹� Ž�� ���� ��
                if (visited[vec] == true) { continue; }

                //Ž�� ������ ��
                else
                {
                    //�Ǽ�������Ʈ Ž��
                    Collider[] col = Physics.OverlapBox(new Vector3(vec.x,0f,vec.y),FoundBoxScale/2, Quaternion.identity,layer);
                    if (col.Length >0)
                    {
                        //�̹� �Ǽ��� �Ϸ�Ǿ����� �Ѿ (�� �� ��� ����� �����ʱ� ������ ������Ʈ�� ����ϸ鼭 ��������)
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
    /// Node�� �θ� ���� ��θ� ��Ž��
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


        //������ ���Ͽ� ��ΰ� �ٲ𶧸� List�� ��θ� �����Ͽ� ��� ��θ� Ž�������ʰ� ��
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
    //BFS�� ���� ����� ��ü ã���� Node�׷����� ���� ��� �ð��� ǥ���ϱ�
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
