using System.Collections;
using NUnit.Framework;
using Parallax.Core.Quest;
using Parallax.Gameplay.Quest;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Parallax.PlayMode.Tests
{
    /// <summary>Fumaça da fatia Quest 1: cena carrega, diretor existe, progresso canônico responde.</summary>
    public sealed class Quest01SmokeTests
    {
        [UnityTest]
        public IEnumerator Quest01SceneLoadsWithDirector()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Quest01_Estacao14", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Quest01_Estacao14 precisa estar no Build Settings.");
            while (!load.isDone)
            {
                yield return null;
            }

            Quest01Director director = Object.FindFirstObjectByType<Quest01Director>();
            Assert.IsNotNull(director, "Quest01Director deve existir na cena.");
            Assert.IsNotNull(director.Chamber);
            Assert.IsNotNull(director.Cargo);
            Assert.IsNotNull(director.Valve);
            Assert.AreEqual(Quest01Stage.Briefing, director.Stage);
        }

        [Test]
        public void CanonProgressProducesGoldWithAllSecondaries()
        {
            var progress = new Quest01Progress();
            progress.Begin(0);
            Assert.IsTrue(progress.CompleteStage(Quest01Stage.OpenDispatchChamber, 1));
            Assert.IsTrue(progress.CompleteStage(Quest01Stage.DeliverCargo, 2));
            Assert.IsTrue(progress.CompleteStage(Quest01Stage.OpenValve, 3));
            Assert.IsTrue(progress.CompleteStage(Quest01Stage.HoldTheLine, 80));
            progress.Mark(Quest01Secondary.Manometro);
            progress.Mark(Quest01Secondary.Relogio);
            progress.Mark(Quest01Secondary.Etiqueta);
            Assert.AreEqual(Quest01Rating.Ouro, progress.Rating);
            Assert.IsTrue(progress.IsComplete);
        }
    }
}
