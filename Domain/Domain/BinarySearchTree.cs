using System;
using System.Collections.Generic;

namespace Domain.Domain
{
    public class Node<T>
    {
        public T Data;
        public Node<T> Left;
        public Node<T> Right;

        public Node(T data) { Data = data; }
    }

    public class BinarySearchTree<T>
    {
        private readonly Func<T, T, int> _getCompareValueFunc;

        public Node<T> Root { get; private set; }

        public BinarySearchTree(Func<T, T, int> getCompareValueFunc)
        {
            _getCompareValueFunc = getCompareValueFunc;
        }

        public void AddNode(T data)
        {
            var newItem = new Node<T>(data);
            if (Root == null)
            {
                Root = newItem;
            }
            else
            {
                var current = Root;
                Node<T> parent;
                while (current != null)
                {
                    parent = current;

                    if (_getCompareValueFunc(data, current.Data) < 0)
                    {
                        current = current.Left;
                        if (current == null)
                            parent.Left = newItem;
                    }
                    else
                    {
                        current = current.Right;
                        if (current == null)
                            parent.Right = newItem;
                    }
                }
            }
        }

        // Function to traverse binary tree without recursion and stack
        public IEnumerable<T> MorrisTraversal(Node<T> root = null)
        {
            if (root == null) root = Root;
            if (root == null) yield break;

            Node<T> current = root, pre;
            while (current != null)
            {
                if (current.Left == null)
                {
                    yield return current.Data;
                    current = current.Right;
                }
                else
                {
                    // Find the inorder predecessor of current 
                    pre = current.Left;
                    while (pre.Right != null && pre.Right != current)
                        pre = pre.Right;

                    // Make current as right child of its inorder predecessor
                    if (pre.Right == null)
                    {
                        pre.Right = current;
                        current = current.Left;
                    }

                    // Revert the changes made in if part to restore the original tree i.e., fix the right child of predecssor
                    else
                    {
                        pre.Right = null;
                        yield return current.Data;
                        current = current.Right;
                    } 
                } 
            } 
        }
    }
}