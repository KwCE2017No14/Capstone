using System.Collections.Generic;
using UnityEngine;

//에디터상에서 경로를 육안으로 확인이 가능하도록 하는 클래스
public class Path : MonoBehaviour {
    public int color_num=0;
    public List<Transform> ListOfNodes; //Transform의 List.
    public bool isReversed = false; //정방향 true, 역방향 false
    public bool isLoop = false; //경로 그림의 루프 여부

    //OnDrawGizmos는 에디터에서 동작하는 중에 호출됨. => 플레이 안해도 적용됨.
    public void OnDrawGizmos()
    {
        Transform[] pathTransforms = GetComponentsInChildren<Transform>(); //현재 오브젝트에 포함된 차일드들의 Transform으로 이루어진 배열 리턴.
        ListOfNodes = new List<Transform>(); //Transform 타입의 List 생성.
        int index;

        //자식 오브젝트 개수만큼 반복.
        for (int i = 0; i < pathTransforms.Length; i++)
        { //정방향이면 순서대로, 역방향이면 맨 마지막 노드부터 List에 추가한다.
            index = (isReversed == false) ? i : pathTransforms.Length - 1 - i;
            if (pathTransforms[index] != transform)
                ListOfNodes.Add(pathTransforms[index]);
        }

        //List에 들어있는 개수만큼 반복.
        for (int i = 0; i < ListOfNodes.Count; i++)
        {
            Vector3 cur = ListOfNodes[i].position; //현재 노드의 position.
            Vector3 prev = Vector3.zero;  //이전 노드의 position.
            if (i == 0)
            {
                prev = (isLoop == false) ? cur : ListOfNodes[ListOfNodes.Count - 1].position;
            }
            else
                prev = ListOfNodes[i - 1].position;

            //기즈모의 색을 color_num 변수의 값에 따라 결정한다.
            switch (color_num) {
                case 0:
                    Gizmos.color = Color.white;
                    break;
                case 1:
                    Gizmos.color = Color.red;
                    break;
                case 2:
                    Gizmos.color = Color.blue;
                    break;
                case 3:
                    Gizmos.color = Color.yellow;
                    break;
                case 4:
                    Gizmos.color = Color.black;
                    break;
                case 5:
                    Gizmos.color = Color.cyan;
                    break;
                case 6:
                    Gizmos.color = Color.green;
                    break;
                case 7:
                    Gizmos.color = Color.magenta;
                    break;
                default:
                    Gizmos.color = Color.white;
                    break;
            }
            //prev와 cur사이에 선을 그리고, cur의 위치에 1f크기의 구를 그린다
            Gizmos.DrawLine(prev, cur);
            Gizmos.DrawWireSphere(cur, 1f);

        }
    }
}
