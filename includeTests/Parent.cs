using System.Collections.Generic;

namespace includeTests
{
    public class Parent
    {
        public Parent()
        {
            Children = new List<Child>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Child> Children { get; set; } 
    }
}