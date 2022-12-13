using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeographyRouter
{
    public class Config
    {
        public readonly bool CheckAllElementsConnectedInNode;

        public Config(bool checkAllElementsConnectedInNode)
        {
            CheckAllElementsConnectedInNode = checkAllElementsConnectedInNode;
        }
    }
}
