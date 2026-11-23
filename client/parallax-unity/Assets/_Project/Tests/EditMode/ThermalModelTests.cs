using NUnit.Framework;
using Parallax.Core.Thermal;

namespace Parallax.Core.Tests
{
    public sealed class ThermalModelTests
    {
        [Test]
        public void NeverDropsBelowTritonAmbient()
        {
            var model = new ThermalModel(ThermalSettings.Default, 100f);
            model.ApplyCryo(10000f);
            Assert.AreEqual(ThermalConstants.TritonAmbientKelvin, model.CurrentKelvin, 1e-4f);
        }

        [Test]
        public void PassiveCoolingMovesTowardAmbient()
        {
            var model = new ThermalModel(ThermalSettings.Default, 300f);
            model.Step(10f);
            Assert.AreEqual(300f - 60f, model.CurrentKelvin, 1e-3f);
        }

        [Test]
        public void HeatingPastThresholdRaisesOverheatOnce()
        {
            var model = new ThermalModel(ThermalSettings.Default, 700f);
            ThermalEvents first = model.ApplyHeat(100f);
            ThermalEvents second = model.ApplyHeat(10f);
            Assert.IsTrue((first & ThermalEvents.BecameOverheated) != 0);
            Assert.IsTrue((second & ThermalEvents.BecameOverheated) == 0);
            Assert.IsTrue(model.IsOverheated);
        }

        [Test]
        public void CryoOnOverheatedTargetTriggersThermalShock()
        {
            var model = new ThermalModel(ThermalSettings.Default, 800f);
            ThermalEvents events = model.ApplyCryo(420f);
            Assert.IsTrue((events & ThermalEvents.ThermalShock) != 0);
            Assert.AreEqual(420f, model.LastShockDropKelvin, 1e-4f);
        }

        [Test]
        public void SmallCryoOnOverheatedTargetDoesNotShock()
        {
            var model = new ThermalModel(ThermalSettings.Default, 800f);
            ThermalEvents events = model.ApplyCryo(100f);
            Assert.IsTrue((events & ThermalEvents.ThermalShock) == 0);
        }

        [Test]
        public void ThermalCellLosesTwoPercentPerDay()
        {
            Assert.AreEqual(98f, ThermalModel.ComputeThermalCellCharge(100f, 1f), 1e-3f);
            Assert.AreEqual(100f * 0.98f * 0.98f, ThermalModel.ComputeThermalCellCharge(100f, 2f), 1e-3f);
            Assert.AreEqual(100f, ThermalModel.ComputeThermalCellCharge(100f, -3f), 1e-3f);
        }
    }
}
