using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace includeTests
{
    using System.Data.Entity.Infrastructure;

    [TestFixture]
    public class AsyncMoqTests
    {
        private static Mock<IDbSet<T>> CreateMockSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<IDbSet<T>>();

            mockSet.Setup(m => m.Expression).Returns(data.Expression);
            mockSet.Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockSet.As<IDbAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator())
            .Returns(new TestDbAsyncEnumerator<T>(data.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<T>(data.Provider));

            return mockSet;
        }

        [Test]
        public async void CanUseAsyncMocks()
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

            var queryTask = await mockContext.Object.Parents.FirstAsync();
            queryTask.Should().NotBeNull();

            var andAnotherTask = await mockContext.Object.Parents.SingleOrDefaultAsync();
            andAnotherTask.Should().NotBeNull();
        }

        [Test]
        public async void CanUseIncludeWithAsyncMocks()
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

            var queryTask = await mockContext.Object.Parents.Include(p => p.Children).FirstAsync();

            queryTask.Should().NotBeNull();
        }

    }
}
