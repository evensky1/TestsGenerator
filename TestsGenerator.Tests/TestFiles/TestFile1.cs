namespace HelloWorld
{
    class Program
    {
        private string str = "Hello";
                          
        static void MainMethod(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }        
    }
}

namespace MyHelloWorld.AnotherOne
{
    class MyAnotherProgram
    {
        public void MethodFromAnotherClass() 
        {
            Console.WriteLine("Hello, World!");
        }     
    }
    class Something
    {
        public void Method() 
        {
            Console.WriteLine("Hello, World!");
        }     
    }
}