using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;

namespace includeTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CanDoDatabaseyThings()
        {
            var db = new TestContext();
            db.Parents.Add(new Parent());
            db.Parents.Should().NotBeEmpty();
        }

        [Test]
        public void CanUseIncludeForReal()
        {
            var db = new TestContext();
            var entity = new Parent();
            entity.Children.Add(new Child());
            db.Parents.Add(entity);
            var parents = db.Parents.Include(p=>p.Children);
            parents.Should().NotBeEmpty();
            parents.First().Children.Should().NotBeEmpty();
        }

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

            mockContext.Parents.Should().NotBeEmpty();
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

            mockContext.Parents.Should().HaveCount(1);
            mockContext.Children.Should().HaveCount(1);

            mockContext.Parents.First().Children.FirstOrDefault().Should().NotBeNull();

            var query = mockContext.Parents.Include(p=>p.Children).Select(pc => pc);

            query.Should().NotBeNull().And.HaveCount(1);
            query.First().Children.Should().NotBeEmpty().And.HaveCount(1);

        }

    }
}
