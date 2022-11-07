using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace DefaultNamespace;

    public class RootClass
    {
        public void Method1()
        {
            Console.WriteLine("Hello, World!");
        }

        class TopChildClass
        {
            public void Method2()
            {
                Console.WriteLine("Hello, World!");
            }
            
            class FirstChildClass
            {
                public void Method3()
                {
                    Console.WriteLine("Hello, World!");
                }
            }
            
            class SecondChildClass
            {
                public void Method3()
                {
                    Console.WriteLine("Hello, World!");
                }
            }
        }
        
        class AnotherTopChildClass
        {
            public void Method3()
            {
                Console.WriteLine("Hello, World!");
            }
        }
    }