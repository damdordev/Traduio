using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Traduio.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Traduio
{
    public class TranslationManagerTest
    {
        private readonly Dictionary<string, string> TestTranslations = new()
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "common", "value3" },
        };
        
        private readonly Dictionary<string, string> OtherTestTranslations = new()
        {
            { "otherKey1", "otherValue1" },
            { "otherKey2", "otherValue2" },
            { "common", "otherValue3" },
        };

        [UnityTest]
        public IEnumerator TestIfEmptyTranslationManagerReturnsEmptyTranslation()
            => UniTask.ToCoroutine(async () =>
            {
                var manager = new TranslationManager();
                Assert.AreEqual("", manager.GetTranslation("key1"));
                Assert.AreEqual("", manager.GetTranslation("key2"));
            });
        
        [UnityTest]
        public IEnumerator TestIfemptyTranslationManagerReturnsImmediatelyWaitForLoad()
            => UniTask.ToCoroutine(async () =>
            {
                var manager = new TranslationManager();
                var frame = Time.frameCount;
                await manager.WaitForLoad();
                Assert.AreEqual(frame, Time.frameCount);
            });
        
        [UnityTest]
        [TestCase("key1",  "value1", 0f, ExpectedResult = null)]
        [TestCase("key2", "value2", 0f, ExpectedResult = null)]
        [TestCase("key3", "", 0f, ExpectedResult = null)]
        [TestCase("key1",  "value1", 0.2f, ExpectedResult = null)]
        [TestCase("key2", "value2", 0.2f, ExpectedResult = null)]
        [TestCase("key3", "", 0.2f, ExpectedResult = null)]
        public IEnumerator TestSimpleGetTranslation(string key, string expected, float waitTime)
            => UniTask.ToCoroutine(async () =>
            {
                var manager = new TranslationManager();
                var loader = new TestTranslationLoader(TestTranslations, waitTime);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();

                Assert.AreEqual(expected, manager.GetTranslation(key));
            });

        [UnityTest]
        public IEnumerator TestIfSynchronousLoaderIsLoadedInTheSameFrame()
            => UniTask.ToCoroutine(async () =>
            {
                var manager = new TranslationManager();
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                manager.AddLoader(loader);
                manager.Load();
                var frame = Time.frameCount;
                await manager.WaitForLoad();
                Assert.AreEqual(Time.frameCount, frame);
            });
        
        [UnityTest]
        public IEnumerator TestIfAsynchronousLoaderIsNotLoadedInTheSameFrame()
            => UniTask.ToCoroutine(async () =>
            {
                var manager = new TranslationManager();
                var loader = new TestTranslationLoader(TestTranslations, 0.1f);
                manager.AddLoader(loader);
                manager.Load();
                var frame = Time.frameCount;
                await manager.WaitForLoad();
                Assert.AreNotEqual(Time.frameCount, frame);
            });
        
        [UnityTest]
        public IEnumerator TestTwoLoaders()
            => UniTask.ToCoroutine(async () =>
            {
                var manager = new TranslationManager();
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("value1", manager.GetTranslation("key1"));
                Assert.AreEqual("value2", manager.GetTranslation("key2"));
                Assert.AreEqual("otherValue1", manager.GetTranslation("otherKey1"));
                Assert.AreEqual("otherValue2", manager.GetTranslation("otherKey2"));
                Assert.AreEqual("value3", manager.GetTranslation("common"));
            });

    }
}