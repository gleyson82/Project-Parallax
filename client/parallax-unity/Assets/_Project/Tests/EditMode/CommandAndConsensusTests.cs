using NUnit.Framework;
using Parallax.Core.Commands;
using Parallax.Core.Roles;

namespace Parallax.Core.Tests
{
    public sealed class CommandAndConsensusTests
    {
        [Test]
        public void AuxiliaryForHumanClassIsUnavailable()
        {
            var queue = new CommandQueueModel();
            queue.SetHumanClasses(new[] { ParallaxClass.Goliath });
            Assert.AreEqual(CommandRejection.AuxiliaryUnavailable, queue.Mark(AuxiliaryId.Macaco, 1));
            Assert.AreEqual(CommandRejection.None, queue.Mark(AuxiliaryId.Pipa, 2));
        }

        [Test]
        public void EachAuxiliaryExecutesOnlyOneVerb()
        {
            var queue = new CommandQueueModel();
            queue.SetHumanClasses(new ParallaxClass[0]);
            Assert.AreEqual(CommandRejection.None, queue.Mark(AuxiliaryId.Pipa, 1));
            Assert.AreEqual(CommandRejection.AuxiliaryAlreadyQueued, queue.Mark(AuxiliaryId.Pipa, 2));
            Assert.AreEqual(AuxiliaryVerb.RiseAndShow, queue.Commands[0].Verb);
        }

        [Test]
        public void NothingExecutesBeforeConfirmation()
        {
            var queue = new CommandQueueModel();
            queue.SetHumanClasses(new[] { ParallaxClass.Cipher });
            queue.Mark(AuxiliaryId.Macaco, 1);
            Assert.IsFalse(queue.BeginExecution());
            Assert.AreEqual(CommandRejection.None, queue.Confirm());
            Assert.IsTrue(queue.BeginExecution());
            Assert.AreEqual(CommandRejection.NotInRehearsal, queue.Mark(AuxiliaryId.Pipa, 2));
        }

        [Test]
        public void FailureShowsWhereAndAllowsRehearsal()
        {
            var queue = new CommandQueueModel();
            queue.SetHumanClasses(new[] { ParallaxClass.Cipher });
            queue.Mark(AuxiliaryId.Macaco, 1);
            queue.Mark(AuxiliaryId.Pipa, 2);
            queue.Confirm();
            queue.BeginExecution();
            queue.ReportResult(1, false);
            Assert.AreEqual(CommandQueuePhase.Failed, queue.Phase);
            Assert.AreEqual(1, queue.FailedIndex);
            queue.Rehearse();
            Assert.AreEqual(2, queue.Commands.Count);
            Assert.AreEqual(CommandRejection.None, queue.Unmark(1));
            Assert.AreEqual(0, queue.Commands[0].Order);
        }

        [Test]
        public void JoiningHumanRemovesTheirAuxiliaryFromQueue()
        {
            var queue = new CommandQueueModel();
            queue.SetHumanClasses(new[] { ParallaxClass.Goliath });
            queue.Mark(AuxiliaryId.Pipa, 1);
            queue.Mark(AuxiliaryId.CartuchoOverride, 2);
            queue.SetHumanClasses(new[] { ParallaxClass.Goliath, ParallaxClass.Phantom });
            Assert.AreEqual(1, queue.Commands.Count);
            Assert.AreEqual(AuxiliaryId.CartuchoOverride, queue.Commands[0].Auxiliary);
            Assert.AreEqual(0, queue.Commands[0].Order);
        }

        [Test]
        public void TriHandsRequiresEveryHumanSimultaneously()
        {
            var hands = new TriHandsConsensus(0.4);
            hands.SetHuman(ParallaxClass.Goliath, true);
            hands.SetHuman(ParallaxClass.Phantom, true);
            hands.Press(ParallaxClass.Goliath, 0.0);
            Assert.IsFalse(hands.IsComplete(1.0));
            hands.Press(ParallaxClass.Phantom, 1.0);
            Assert.IsFalse(hands.IsComplete(1.2));
            Assert.IsTrue(hands.IsComplete(1.4));
            hands.Release(ParallaxClass.Goliath);
            Assert.IsFalse(hands.IsComplete(2.0));
        }

        [Test]
        public void TriHandsIgnoresAuxiliarySeats()
        {
            var hands = new TriHandsConsensus(0.4);
            hands.SetHuman(ParallaxClass.Cipher, true);
            hands.Press(ParallaxClass.Goliath, 0.0);
            Assert.IsFalse(hands.IsPressed(ParallaxClass.Goliath));
            hands.Press(ParallaxClass.Cipher, 0.0);
            Assert.IsTrue(hands.IsComplete(0.5));
        }

        [Test]
        public void MemoryWindowClosesAfterDuration()
        {
            var window = new MemoryWindow(3, 12.0);
            window.Activate(0, 0.0);
            window.Activate(1, 5.0);
            window.Activate(2, 11.0);
            Assert.IsTrue(window.AllOpen(12.0));
            Assert.IsFalse(window.AllOpen(12.5));
        }

        [Test]
        public void RolesMapOneToOne()
        {
            Assert.AreEqual(AuxiliaryId.Macaco, ParallaxRoles.AuxiliaryFor(ParallaxClass.Goliath));
            Assert.AreEqual(ParallaxClass.Cipher, ParallaxRoles.ClassCoveredBy(AuxiliaryId.CartuchoOverride));
            Assert.AreEqual(CorporationId.Kinesys, ParallaxRoles.LockOperatedBy(ParallaxClass.Phantom));
        }
    }
}
