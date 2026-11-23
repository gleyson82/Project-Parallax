using NUnit.Framework;
using Parallax.Core.Audio;
using Parallax.Core.Session;

namespace Parallax.Core.Tests
{
    public sealed class BeatAndScalingTests
    {
        private static readonly bool[] Pattern = { true, false, true, true, false, false, true, false };

        [Test]
        public void StepDurationIsEighthNoteAtBpm()
        {
            var schedule = new BeatSchedule(96.0, Pattern, 10.0);
            Assert.AreEqual(60.0 / 96.0 / 2.0, schedule.StepDuration, 1e-12);
        }

        [Test]
        public void StepTimesAreExactMultiplesFromStart()
        {
            var schedule = new BeatSchedule(120.0, Pattern, 5.0);
            Assert.AreEqual(5.0 + 1000 * 0.25, schedule.TimeOfStep(1000), 1e-9);
        }

        [Test]
        public void NextStepAtOrAfterRoundsUp()
        {
            var schedule = new BeatSchedule(120.0, Pattern, 0.0);
            Assert.AreEqual(0, schedule.NextStepAtOrAfter(-1.0));
            Assert.AreEqual(4, schedule.NextStepAtOrAfter(1.0));
            Assert.AreEqual(5, schedule.NextStepAtOrAfter(1.01));
        }

        [Test]
        public void AccentsFollowPatternCyclically()
        {
            var schedule = new BeatSchedule(96.0, Pattern, 0.0);
            Assert.IsTrue(schedule.IsAccent(0));
            Assert.IsFalse(schedule.IsAccent(1));
            Assert.IsTrue(schedule.IsAccent(8));
            Assert.IsTrue(schedule.IsAccent(14));
        }

        [Test]
        public void FloorNoteSpansFortyToSixtyHertz()
        {
            Assert.AreEqual(40f, FloorNote.FrequencyFor(0f), 1e-4f);
            Assert.AreEqual(60f, FloorNote.FrequencyFor(1f), 1e-4f);
            Assert.AreEqual(40f, FloorNote.FrequencyFor(-2f), 1e-4f);
        }

        [TestCase(3, 12f, 1f)]
        [TestCase(2, 18f, 0.8f)]
        [TestCase(1, 24f, 0.65f)]
        public void SessionScalingMatchesCanonTable(int humans, float window, float enemies)
        {
            SessionScalingTable table = SessionScalingTable.Canon;
            Assert.AreEqual(window, SessionScaling.FinalActionWindowSeconds(humans, table), 1e-4f);
            Assert.AreEqual(enemies, SessionScaling.EnemyMultiplier(humans, table), 1e-4f);
            Assert.AreEqual(3 - humans, SessionScaling.AuxiliariesNeeded(humans));
        }
    }
}
