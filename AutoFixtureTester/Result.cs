/**
*
*       AT-CTF-PCB-001
*       Helper class for storing test failure data 
*
*       Author: John Glatts
*       Date:   8/21/2025
*/
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
