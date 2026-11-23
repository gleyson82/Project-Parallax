using NUnit.Framework;
using Parallax.Core.Combat;
using Parallax.Core.Commands;
using Parallax.Core.Quest;
using Parallax.Core.Session;

namespace Parallax.Core.Tests
{
    public sealed class Quest01CoreTests
    {
        [Test]
        public void WeaponLocksAtFullHeatAndUnlocksBelowThreshold()
        {
            var heat = new WeaponHeat(0.25f, 0.5f, 0.3f);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(heat.TryFire());
            }

            Assert.IsTrue(heat.Overheated);
            Assert.IsFalse(heat.TryFire());
            heat.Cool(1f);
            Assert.IsTrue(heat.Overheated, "0,5 ainda está acima do limiar de 0,3.");
            heat.Cool(0.5f);
            Assert.IsFalse(heat.Overheated);
            Assert.IsTrue(heat.TryFire());
        }

        [Test]
        public void VitalsDownAndRevive()
        {
            var vitals = new VitalsModel(100f);
            Assert.IsFalse(vitals.ApplyDamage(60f));
            Assert.IsTrue(vitals.ApplyDamage(60f));
            Assert.IsTrue(vitals.IsDowned);
            Assert.IsFalse(vitals.ApplyDamage(10f), "Caído não cai de novo.");
            vitals.Revive(0.4f);
            Assert.IsFalse(vitals.IsDowned);
            Assert.AreEqual(40f, vitals.Integrity, 1e-4f);
        }

        [Test]
        public void QuestAdvancesOnlyInOrder()
        {
            var quest = new Quest01Progress();
            Assert.IsFalse(quest.CompleteStage(Quest01Stage.OpenDispatchChamber, 1));
            quest.Begin(10);
            Assert.IsFalse(quest.CompleteStage(Quest01Stage.OpenValve, 11), "Etapa fora de ordem não avança.");
            Assert.IsTrue(quest.CompleteStage(Quest01Stage.OpenDispatchChamber, 20));
            Assert.IsTrue(quest.CompleteStage(Quest01Stage.DeliverCargo, 30));
            Assert.IsTrue(quest.CompleteStage(Quest01Stage.OpenValve, 40));
            Assert.IsTrue(quest.CompleteStage(Quest01Stage.HoldTheLine, 130));
            Assert.IsTrue(quest.IsComplete);
            Assert.AreEqual(120, quest.ElapsedSeconds(999), 1e-9);
        }

        [Test]
        public void RatingFollowsSecondaries()
        {
            var quest = new Quest01Progress();
            Assert.AreEqual(Quest01Rating.Prata, quest.Rating, "Fogo Mínimo começa intacto.");
            quest.BreakFogoMinimo();
            Assert.AreEqual(Quest01Rating.Bronze, quest.Rating);
            quest.Mark(Quest01Secondary.Manometro);
            quest.Mark(Quest01Secondary.Relogio);
            quest.Mark(Quest01Secondary.Etiqueta);
            Assert.AreEqual(Quest01Rating.Prata, quest.Rating);

            var perfect = new Quest01Progress();
            perfect.Mark(Quest01Secondary.Manometro);
            perfect.Mark(Quest01Secondary.Relogio);
            perfect.Mark(Quest01Secondary.Etiqueta);
            Assert.AreEqual(Quest01Rating.Ouro, perfect.Rating);
            Assert.IsFalse(perfect.Mark(Quest01Secondary.FogoMinimo), "Fogo Mínimo não se marca, só se perde.");
        }

        [Test]
        public void CoresAverageCharge()
        {
            var quest = new Quest01Progress();
            quest.CollectCore(1f);
            quest.CollectCore(0.5f);
            Assert.AreEqual(2, quest.CoresCollected);
            Assert.AreEqual(0.75f, quest.AverageCoreCharge, 1e-5f);
        }

        [TestCase(3, 20f, 3)]
        [TestCase(2, 30f, 3)]
        [TestCase(1, 40f, 2)]
        public void TuningScalesWithSession(int humans, float window, int waves)
        {
            Quest01Tuning tuning = Quest01Tuning.Canon;
            SessionScalingTable table = SessionScalingTable.Canon;
            Assert.AreEqual(window, tuning.DispatchWindowSeconds(humans, table), 1e-4f);
            int[] sizes = tuning.WavesFor(humans, table);
            Assert.AreEqual(waves, sizes.Length);
            Assert.GreaterOrEqual(sizes[0], 1);
        }

        [Test]
        public void MemoryWindowReportsEarliestDeadline()
        {
            var window = new MemoryWindow(3, 20.0);
            Assert.IsFalse(window.TryGetDeadline(0, out _));
            window.Activate(1, 5.0);
            window.Activate(0, 8.0);
            Assert.IsTrue(window.TryGetDeadline(9.0, out double deadline));
            Assert.AreEqual(25.0, deadline, 1e-9);
            Assert.AreEqual(2, window.OpenCount(9.0));
            Assert.AreEqual(1, window.OpenCount(26.0));
        }
    }
}
