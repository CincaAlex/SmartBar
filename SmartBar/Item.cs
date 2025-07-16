using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBar
{
    public class Item
    {
        private string name;
        private string type;
        private string path;

        public Item(string name, string type, string path)
        {
            this.name = name;
            this.type = type;
            this.path = path;
        }

        public string getName()
        {
            return this.name;
        }

        public string getType()
        {
            return this.type;
        }

        public string getPath()
        {
            return this.path;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
