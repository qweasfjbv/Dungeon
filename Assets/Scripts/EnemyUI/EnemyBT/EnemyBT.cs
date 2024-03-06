using EnemyUI.BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EnemyUI.BehaviorTree
{
    public class EnemyBT : Tree
    {
        public override Node SetupRoot()
        {
            Node root = new Selector(new List<Node> {
                    new Sequence(new List<Node>
                    {
                        new Search(),
                        new Move()
                    }),
                    new Attack()
                });



            return root;
        }
    }

    public class Search : Node {


        public Search()
        {
            // 필요한 초기화
        }


    }
    public class Move : Node
    {


        public Move()
        {
            // 필요한 초기화
        }


    }
    public class Attack : Node
    {


        public Attack()
        {
            // 필요한 초기화
        }


    }


}