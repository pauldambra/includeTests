using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace includeTests
{
    [TestFixture]
    public class MoqTests
    {
        private static Mock<IDbSet<T>> CreateMockSet<T>(IQueryable<T> childlessParents) where T : class
        {
            var mockSet = new Mock<IDbSet<T>>();

            mockSet.Setup(m => m.Provider).Returns(childlessParents.Provider);
            mockSet.Setup(m => m.Expression).Returns(childlessParents.Expression);
            mockSet.Setup(m => m.ElementType).Returns(childlessParents.ElementType);
            mockSet.Setup(m => m.GetEnumerator()).Returns(childlessParents.GetEnumerator());
            return mockSet;
        }

        [Test]
        public void CanDoMockDatabaseyThings()
        {
            var childlessParents = new List<Parent>{new Parent()}.AsQueryable();

            var mockContext = new Mock<TestContext>();

            var mockParentSet = CreateMockSet(childlessParents);

            mockContext.SetupGet(mc=>mc.Parents).Returns(mockParentSet.Object);

            mockContext.Object.Parents.Select(p => p).Should().NotBeEmpty().And.HaveCount(1);
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

            var mockContext = new Mock<TestContext>();

            var mockParentSet = CreateMockSet(parents);
            var mockChildSet = CreateMockSet(children);

            mockContext.SetupGet(mc => mc.Parents).Returns(mockParentSet.Object);
            mockContext.SetupGet(mc => mc.Children).Returns(mockChildSet.Object);

            mockContext.Object.Parents.Should().HaveCount(1);
            mockContext.Object.Children.Should().HaveCount(1);

            mockContext.Object.Parents.First().Children.FirstOrDefault().Should().NotBeNull();

            var query = mockContext.Object.Parents.Include(p => p.Children).Select(p => p);

            query.Should().NotBeNull().And.HaveCount(1);
            query.First().Children.Should().NotBeEmpty().And.HaveCount(1);
        }

    }
}
