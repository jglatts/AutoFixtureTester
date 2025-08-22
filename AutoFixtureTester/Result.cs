using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFixtureTester
{
    class Result
    {
        public bool hasFailure;
        public List<string> failures;

        public Result()
        {
            hasFailure = false;
            failures= new List<string>();
        }
    }
}
