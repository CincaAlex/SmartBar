using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBar
{
    public class Node
    {
        private string value;
        private Node left;
        private Node right;
        private Node parrent;

        public Node(string value)
        {
            this.value = value;
            this.left = null;
            this.right = null;
            this.parrent = null;
        }

        public void setLeft(Node node)
        {
            this.left = node;
        }

        public void setRight(Node node)
        {
            this.right = node;
        }

        public void setParrent(Node node)
        {
            this.parrent = node;
        }
    }
}
