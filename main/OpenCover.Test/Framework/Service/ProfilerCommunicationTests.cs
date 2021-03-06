﻿using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Service;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Service
{
    [TestFixture]
    internal class ProfilerCommunicationTests :
        UnityAutoMockContainerBase<IProfilerCommunication, ProfilerCommunication>
    {
        [Test]
        public void TrackAssembly_Adds_AssemblyToModel_If_FilterUseAssembly_Returns_True()
        {
            // arrange
            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            var mockModelBuilder = new Mock<IInstrumentationModelBuilder>();
            Container.GetMock<IInstrumentationModelBuilderFactory>()
               .Setup(x => x.CreateModelBuilder(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(mockModelBuilder.Object);

            mockModelBuilder
                .SetupGet(x => x.CanInstrument)
                .Returns(true);

            // act
            var track = Instance.TrackAssembly("moduleName", "assemblyName");
            
            // assert
            Assert.IsTrue(track);
            Container.GetMock<IPersistance>()
                .Verify(x=>x.PersistModule(It.IsAny<Module>()), Times.Once());
        }

        [Test]
        public void TrackAssembly_DoesntAdd_AssemblyToModel_If_FilterUseAssembly_Returns_False()
        {
            // arrange
            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(false);

            // act
            var track = Instance.TrackAssembly("moduleName", "assemblyName");

            // assert
            Assert.IsFalse(track);
            Container.GetMock<IPersistance>()
                .Verify(x => x.PersistModule(It.IsAny<Module>()), Times.Never());
        }

        [Test]
        public void TrackAssembly_DoesntAdd_AssemblyToModel_If_CanInstrument_Returns_False()
        {
            // arrange
            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            var mockModelBuilder = new Mock<IInstrumentationModelBuilder>();
            Container.GetMock<IInstrumentationModelBuilderFactory>()
               .Setup(x => x.CreateModelBuilder(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(mockModelBuilder.Object);

            mockModelBuilder
                .SetupGet(x => x.CanInstrument)
                .Returns(false);

            // act
            var track = Instance.TrackAssembly("moduleName", "assemblyName");

            // assembly
            Assert.IsFalse(track);
            Container.GetMock<IPersistance>()
                .Verify(x => x.PersistModule(It.IsAny<Module>()), Times.Never());

        }

        [Test]
        public void Stopping_Forces_Model_Finalise()
        {
            // arrange

            // act
            Instance.Stopping();

            // assert
            Container.GetMock<IPersistance>()
                .Verify(x => x.Commit(), Times.Once());
        }

        [Test]
        public void GetSequencePoints_Returns_False_When_No_Data_In_Model()
        {
            // arrange
            InstrumentationPoint[] points;
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(false)
                .Verifiable();
            Container.GetMock<IPersistance>()
               .Setup(x => x.IsTracking(It.IsAny<string>()))
               .Returns(true);
            Container.GetMock<IFilter>()
               .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);

            // act
            InstrumentationPoint[] instrumentPoints;
            var result = Instance.GetSequencePoints("moduleName", "moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsFalse(result);
            Container.GetMock<IPersistance>().Verify();
        }

        [Test]
        public void GetSequencePoints_Returns_False_When_Not_Tracking_Assembly()
        {
            // arrange
            InstrumentationPoint[] points;
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(false);
            Container.GetMock<IPersistance>()
               .Setup(x => x.IsTracking(It.IsAny<string>()))
               .Returns(false);
           
            // act
            InstrumentationPoint[] instrumentPoints;
            var result = Instance.GetSequencePoints("moduleName", "moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsFalse(result);
            Container.GetMock<IPersistance>()
                 .Verify(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points), Times.Never());
        }

        [Test]
        public void GetSequencePoints_Returns_False_When_Not_Tracking_Class()
        {
            // arrange
            InstrumentationPoint[] points;
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(false);
            Container.GetMock<IPersistance>()
               .Setup(x => x.IsTracking(It.IsAny<string>()))
               .Returns(true);
            Container.GetMock<IFilter>()
               .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(false);

            // act
            InstrumentationPoint[] instrumentPoints;
            var result = Instance.GetSequencePoints("moduleName", "moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsFalse(result);
            Container.GetMock<IPersistance>()
                 .Verify(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points), Times.Never());
        }

        [Test]
        public void GetSequencePoints_Returns_SequencePoints_When_Data_In_Model()
        {
            // arrange
            var points = new[] { new InstrumentationPoint() };
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetSequencePointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(true)
                .Verifiable();
            Container.GetMock<IPersistance>()
               .Setup(x => x.IsTracking(It.IsAny<string>()))
               .Returns(true);
            Container.GetMock<IFilter>()
               .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);

            // act
            InstrumentationPoint[] instrumentPoints;
            var result = Instance.GetSequencePoints("moduleName", "moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual(points.GetLength(0), instrumentPoints.GetLength(0));
            Container.GetMock<IPersistance>().Verify();
        }

        [Test]
        public void GetBranchPoints_Returns_False_When_No_Data_In_Model()
        {
            // arrange
            BranchPoint[] points;
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetBranchPointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(false)
                .Verifiable();
            Container.GetMock<IPersistance>()
               .Setup(x => x.IsTracking(It.IsAny<string>()))
               .Returns(true);
            Container.GetMock<IFilter>()
               .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);

            // act
            BranchPoint[] instrumentPoints;
            var result = Instance.GetBranchPoints("moduleName", "moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsFalse(result);
            Container.GetMock<IPersistance>().Verify();
        }

        [Test]
        public void GetBranchPoints_Returns_SequencePoints_When_Data_In_Model()
        {
            // arrange
            var points = new[] { new BranchPoint(),  };
            Container.GetMock<IPersistance>()
                .Setup(x => x.GetBranchPointsForFunction(It.IsAny<string>(), It.IsAny<int>(), out points))
                .Returns(true)
                .Verifiable();
            Container.GetMock<IPersistance>()
               .Setup(x => x.IsTracking(It.IsAny<string>()))
               .Returns(true);
            Container.GetMock<IFilter>()
               .Setup(x => x.InstrumentClass(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);

            // act
            BranchPoint[] instrumentPoints;
            var result = Instance.GetBranchPoints("moduleName", "moduleName", 1, out instrumentPoints);

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual(points.GetLength(0), instrumentPoints.GetLength(0));
            Container.GetMock<IPersistance>().Verify();
        }
    }
}
