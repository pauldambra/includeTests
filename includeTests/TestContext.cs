using System.Data.Entity;

namespace includeTests
{
    public class TestContext : DbContext
    {
        public virtual IDbSet<Parent> Parents { get; set; }
        public virtual IDbSet<Child> Children { get; set; } 
    }
}