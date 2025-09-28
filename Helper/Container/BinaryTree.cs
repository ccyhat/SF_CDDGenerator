using System.Collections;

namespace SFTemplateGenerator.Helper.Container
{
    public class BinaryTree<T> : IEnumerable<T> where T : IComparable<T>
    {
        /// <summary>
        /// 二叉树节点类
        /// </summary>
        private class Node
        {
            public T Value { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public Node(T value)
            {
                Value = value;
                Left = null!;
                Right = null!;
            }
        }

        // 根节点
        private Node _root;

        // 节点数量
        public int Count { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BinaryTree()
        {
            _root = null!;
            Count = 0;
        }

        /// <summary>
        /// 检查树是否为空
        /// </summary>
        public bool IsEmpty => _root == null;

        /// <summary>
        /// 清空树
        /// </summary>
        public void Clear()
        {
            _root = null!;
            Count = 0;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="value">要插入的值</param>
        public void Insert(T value)
        {
            if (IsEmpty)
            {
                _root = new Node(value);
            }
            else
            {
                InsertNode(_root, value);
            }
            Count++;
        }

        /// <summary>
        /// 递归插入节点
        /// </summary>
        private void InsertNode(Node current, T value)
        {
            int comparison = value.CompareTo(current.Value);

            // 插入到左子树
            if (comparison < 0)
            {
                if (current.Left == null)
                {
                    current.Left = new Node(value);
                }
                else
                {
                    InsertNode(current.Left, value);
                }
            }
            // 插入到右子树
            else if (comparison > 0)
            {
                if (current.Right == null)
                {
                    current.Right = new Node(value);
                }
                else
                {
                    InsertNode(current.Right, value);
                }
            }
            // 相等的值不插入（可以根据需求修改此行为）
        }

        /// <summary>
        /// 查找元素
        /// </summary>
        /// <param name="value">要查找的值</param>
        /// <returns>如果找到返回true，否则返回false</returns>
        public bool Contains(T value)
        {
            return FindNode(_root, value) != null;
        }

        /// <summary>
        /// 递归查找节点
        /// </summary>
        private Node FindNode(Node current, T value)
        {
            if (current == null)
                return null;

            int comparison = value.CompareTo(current.Value);

            if (comparison == 0)
                return current; // 找到节点
            if (comparison < 0)
                return FindNode(current.Left, value); // 在左子树查找
            return FindNode(current.Right, value); // 在右子树查找
        }

        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="value">要删除的值</param>
        /// <returns>如果删除成功返回true，否则返回false</returns>
        public bool Remove(T value)
        {
            Node parent = null!;
            Node current = _root;
            bool isLeftChild = false;

            // 查找要删除的节点及其父节点
            while (current != null)
            {
                int comparison = value.CompareTo(current.Value);

                if (comparison == 0)
                    break; // 找到要删除的节点

                parent = current;
                if (comparison < 0)
                {
                    current = current.Left;
                    isLeftChild = true;
                }
                else
                {
                    current = current.Right;
                    isLeftChild = false;
                }
            }

            // 如果未找到节点
            if (current == null)
                return false;

            // 情况1：删除的节点是叶子节点
            if (current.Left == null && current.Right == null)
            {
                if (current == _root)
                    _root = null!;
                else if (isLeftChild)
                    parent.Left = null!;
                else
                    parent.Right = null!;
            }
            // 情况2：删除的节点只有一个子节点（右子节点）
            else if (current.Left == null)
            {
                if (current == _root)
                    _root = current.Right;
                else if (isLeftChild)
                    parent.Left = current.Right;
                else
                    parent.Right = current.Right;
            }
            // 情况2：删除的节点只有一个子节点（左子节点）
            else if (current.Right == null)
            {
                if (current == _root)
                    _root = current.Left;
                else if (isLeftChild)
                    parent.Left = current.Left;
                else
                    parent.Right = current.Left;
            }
            // 情况3：删除的节点有两个子节点
            else
            {
                // 找到中序后继节点（右子树中最小的节点）
                Node successor = FindMinNode(current.Right);
                current.Value = successor.Value;

                // 删除后继节点
                RemoveNode(current, current.Right, true);
            }

            Count--;
            return true;
        }

        /// <summary>
        /// 查找最小节点
        /// </summary>
        private Node FindMinNode(Node node)
        {
            Node current = node;
            while (current.Left != null)
            {
                current = current.Left;
            }
            return current;
        }

        /// <summary>
        /// 递归删除节点（用于删除有两个子节点的情况）
        /// </summary>
        private void RemoveNode(Node parent, Node current, bool isLeftChild)
        {
            if (current.Left != null)
            {
                RemoveNode(current, current.Left, true);
            }
            else
            {
                if (isLeftChild)
                    parent.Left = current.Right;
                else
                    parent.Right = current.Right;
            }
        }

        /// <summary>
        /// 前序遍历（根-左-右）
        /// </summary>
        public IEnumerable<T> PreOrderTraversal()
        {
            var result = new List<T>();
            PreOrder(_root, result);
            return result;
        }

        private void PreOrder(Node node, List<T> result)
        {
            if (node != null)
            {
                result.Add(node.Value);
                PreOrder(node.Left, result);
                PreOrder(node.Right, result);
            }
        }

        /// <summary>
        /// 中序遍历（左-根-右）
        /// </summary>
        public IEnumerable<T> InOrderTraversal()
        {
            var result = new List<T>();
            InOrder(_root, result);
            return result;
        }

        private void InOrder(Node node, List<T> result)
        {
            if (node != null)
            {
                InOrder(node.Left, result);
                result.Add(node.Value);
                InOrder(node.Right, result);
            }
        }

        /// <summary>
        /// 后序遍历（左-右-根）
        /// </summary>
        public IEnumerable<T> PostOrderTraversal()
        {
            var result = new List<T>();
            PostOrder(_root, result);
            return result;
        }

        private void PostOrder(Node node, List<T> result)
        {
            if (node != null)
            {
                PostOrder(node.Left, result);
                PostOrder(node.Right, result);
                result.Add(node.Value);
            }
        }

        /// <summary>
        /// 层次遍历
        /// </summary>
        public IEnumerable<T> LevelOrderTraversal()
        {
            var result = new List<T>();
            if (_root == null)
                return result;

            var queue = new Queue<Node>();
            queue.Enqueue(_root);

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();
                result.Add(current.Value);

                if (current.Left != null)
                    queue.Enqueue(current.Left);
                if (current.Right != null)
                    queue.Enqueue(current.Right);
            }

            return result;
        }

        /// <summary>
        /// 获取树的高度
        /// </summary>
        public int Height()
        {
            return CalculateHeight(_root);
        }

        private int CalculateHeight(Node node)
        {
            if (node == null)
                return 0;

            return 1 + Math.Max(CalculateHeight(node.Left), CalculateHeight(node.Right));
        }

        /// <summary>
        /// 实现IEnumerable接口，默认使用中序遍历
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return InOrderTraversal().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
