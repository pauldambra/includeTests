using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;

namespace includeTests
{
    [TestFixture]
    public class RhinoMocksTests
    {
        [Test]
        public void CanDoMockDatabaseyThings()
        {
            var childlessParents = new List<Parent>{new Parent()}.AsQueryable();

            var mockContext = MockRepository.GenerateStub<TestContext>();

            var mockParentSet = MockRepository.GenerateStub<IDbSet<Parent>>();

            mockParentSet.Stub(m => m.Provider).Return(childlessParents.Provider);
            mockParentSet.Stub(m => m.Expression).Return(childlessParents.Expression);
            mockParentSet.Stub(m => m.ElementType).Return(childlessParents.ElementType);
            mockParentSet.Stub(m => m.GetEnumerator()).Return(childlessParents.GetEnumerator());

            mockContext.Parents = mockParentSet;

            mockContext.Parents.Select(p => p).Should().NotBeEmpty();
            mockContext.Parents.Select(p => p).Should().HaveCount(1);
        }

        [Test]
        public void CanUseIncludeWithMocks()
        {
            var child = new Child();
            var parent = new Parent();
            parent.Children.Add(child);

            var parents = new List<Parent>
                {
                    parent
                }.AsQueryable();
            
            var children = new List<Child>
                {
                    child
                }.AsQueryable();

            var mockContext = MockRepository.GenerateStub<TestContext>();

            var mockParentSet = MockRepository.GenerateStub<IDbSet<Parent>>();
            var mockChildSet = MockRepository.GenerateStub<IDbSet<Child>>();

            mockParentSet.Stub(m => m.Provider).Return(parents.Provider);
            mockParentSet.Stub(m => m.Expression).Return(parents.Expression);
            mockParentSet.Stub(m => m.GetEnumerator()).Return(parents.GetEnumerator());

            mockChildSet.Stub(m => m.Provider).Return(children.Provider);
            mockChildSet.Stub(m => m.Expression).Return(children.Expression);
            mockChildSet.Stub(m => m.GetEnumerator()).Return(children.GetEnumerator());

            mockContext.Parents = mockParentSet;
            mockContext.Children = mockChildSet;
//
//            mockContext.Parents.Should().HaveCount(1);
//            mockContext.Children.Should().HaveCount(1);
//
//            mockContext.Parents.First().Children.FirstOrDefault().Should().NotBeNull();

            var query = mockContext.Parents.Include(p=>p.Children).Select(pc => pc);

            query.Should().NotBeNull().And.HaveCount(1);
            query.First().Children.Should().NotBeEmpty().And.HaveCount(1);

        }

    }
}
