using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
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
            { "common", "value3" }
        };
        
        private readonly Dictionary<string, string> OtherTestTranslations = new()
        {
            { "otherKey1", "otherValue1" },
            { "otherKey2", "otherValue2" },
            { "common", "otherValue3" }
        };
        
        private TranslationManager manager;

        [UnityTest]
        public IEnumerator TestIfEmptyTranslationManagerReturnsEmptyTranslation()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                Assert.AreEqual("{missing:key1}", manager.GetTranslation("key1"));
                Assert.AreEqual("{missing:key2}", manager.GetTranslation("key2"));
            });

        [SetUp]
        public void Setup()
        {
            manager = new TranslationManager();
            manager.LogLoaderErrors = false;
        }

        [UnityTest]
        public IEnumerator TestRemovingLoaderBeforeLoad()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.RemoveLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("value1", manager.GetTranslation("key1"));
                Assert.AreEqual("{missing:otherKey1}", manager.GetTranslation("otherKey1"));
            });
        
        [UnityTest]
        public IEnumerator TestRemovingLoaderAfterLoad()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.Load();
                manager.RemoveLoader(loader2);
                await manager.WaitForLoad();
                
                Assert.AreEqual("value1", manager.GetTranslation("key1"));
                Assert.AreEqual("{missing:otherKey1}", manager.GetTranslation("otherKey1"));
            });
        
        [UnityTest]
        public IEnumerator TestRemovingLoaderAfterLoaded()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                manager.RemoveLoader(loader2);
                
                Assert.AreEqual("value1", manager.GetTranslation("key1"));
                Assert.AreEqual("{missing:otherKey1}", manager.GetTranslation("otherKey1"));
            });
        
        [UnityTest]
        public IEnumerator TestIfEmptyTranslationManagerReturnsImmediatelyWaitForLoad()
            => UniTask.ToCoroutine(async () =>
            {
                var frame = Time.frameCount;
                await manager.WaitForLoad();
                Assert.AreEqual(frame, Time.frameCount);
            });
        
        [UnityTest]
        [TestCase("key1",  "value1", 0f, ExpectedResult = null)]
        [TestCase("key2", "value2", 0f, ExpectedResult = null)]
        [TestCase("key3", "{missing:key3}", 0f, ExpectedResult = null)]
        [TestCase("key1",  "value1", 0.1f, ExpectedResult = null)]
        [TestCase("key2", "value2", 0.1f, ExpectedResult = null)]
        [TestCase("key3", "{missing:key3}", 0.1f, ExpectedResult = null)]
        public IEnumerator TestSimpleGetTranslation(string key, string expected, float waitTime)
            => UniTask.ToCoroutine(async () =>
            {
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

        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfLoaderWithExceptionDoesntBreakManager(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new ExceptionTranslationLoader(delay, new ArgumentException());
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad().Timeout(TimeSpan.FromSeconds(2));
            });

        [UnityTest]
        public IEnumerator TestIfRemovingLoaderCancelsLoader()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0.5f);
                manager.AddLoader(loader);
                manager.Load();
                manager.RemoveLoader(loader);
                var frame = Time.frameCount;
                await manager.WaitForLoad();
                Assert.IsTrue(loader.IsCancelled);
                Assert.AreEqual(frame, Time.frameCount);
            });

        [UnityTest]
        public IEnumerator TestIfThrowingExceptionInLoadedDoesntAffectOtherLoaders()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0.5f);
                var loader2 = new ExceptionTranslationLoader(0.2f, new ArgumentException());
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("value1", manager.GetTranslation("key1"));
            });

        [UnityTest]
        public IEnumerator TestIfLoaderIfInvokedOnlyOnce()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0.5f);
                manager.AddLoader(loader);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual(1, loader.LoadCounter);
            });
        
        [UnityTest]
        public IEnumerator TestIfDisposeCancelsLoaders()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0.5f);
                manager.AddLoader(loader);
                manager.Load();
                manager.Dispose();
                var frame = Time.frameCount;
                await manager.WaitForLoad();
                Assert.IsTrue(loader.IsCancelled);
                Assert.AreEqual(frame, Time.frameCount);
            });

        [UnityTest]
        public IEnumerator TestIfCancellingWaitForLoadDoesntAffectLoading()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0.5f);
                var cts = new CancellationTokenSource();
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad(cts.Token);
                cts.Cancel();
                Assert.IsFalse(loader.IsCancelled);
            });

        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfExceptionIsCaughtByHandler(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var exception = new Exception();
                var loader = new ExceptionTranslationLoader(delay, exception);
                var correctlyHandled = false;
                
                manager.SetLoaderErrorHandler((l, e) =>
                {
                    correctlyHandled = l == loader && e == exception;
                });
                
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                Assert.IsTrue(correctlyHandled);
            });
        
        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfNullTranslationsAreCaughtByExceptionHandler(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new NullTranslationLoader(delay);
                var correctlyHandled = false;
                
                manager.SetLoaderErrorHandler((l, e) =>
                {
                    correctlyHandled = l == loader && e is EmptyTranslationException;
                });
                
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                Assert.IsTrue(correctlyHandled);
            });
        
        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckIfEnablingLogLoaderErrorsWorks(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                LogAssert.Expect(LogType.Error, new Regex(".*"));
                manager.LogLoaderErrors = true;
                var loader = new ExceptionTranslationLoader(delay, new ArgumentException());
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
            });
        
        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckIfEnablingLogLoaderErrorsWorksOnNullTranslations(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                LogAssert.Expect(LogType.Error, new Regex(".*"));
                manager.LogLoaderErrors = true;
                var loader = new NullTranslationLoader(delay);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
            });
        
        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckIfLogLoaderIsEnabledByDefault(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                LogAssert.Expect(LogType.Error, new Regex(".*"));
                manager = new TranslationManager();
                var loader = new ExceptionTranslationLoader(delay, new ArgumentException());
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();

                Assert.IsTrue(manager.LogLoaderErrors);
            });

        [UnityTest]
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckIfLogLoaderWorksWithCustomErrorHandler(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                LogAssert.Expect(LogType.Error, new Regex(".*"));
                manager.LogLoaderErrors = true;
                manager.SetLoaderErrorHandler((l, e) => { });
                var loader = new ExceptionTranslationLoader(delay, new ArgumentException());
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();

                Assert.IsTrue(manager.LogLoaderErrors);
            });
        
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckIfManagerSupressExceptionInLoaderErrorHandler(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                manager.SetLoaderErrorHandler((l, e) => throw new ArgumentException());
                var loader = new ExceptionTranslationLoader(delay, new ArgumentException());
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckEmptyKey(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, delay);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                Assert.AreEqual("{missing:}", manager.GetTranslation(""));
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator CheckNullKey(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, delay);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                Assert.AreEqual("{missing:}", manager.GetTranslation(null));
            });

        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestMissingTranslationErrorHandler(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, delay);
                var missingId = "";
                manager.SetMissingTranslationErrorHandler(id =>
                {
                    missingId = id;
                });
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                manager.GetTranslation("non existing");
                
                Assert.AreEqual("non existing", missingId);
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfMissingTranslationErrorHandlerExceptionDoesntCrashManager(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, delay);
                var missingId = "";
                manager.SetMissingTranslationErrorHandler(id =>
                {
                    throw new ArgumentException();
                });
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                manager.GetTranslation("non existing");
            });

        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestTranslationProcessor(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var firstLoader = new TestTranslationLoader(TestTranslations, delay);
                var otherLoader = new TestTranslationLoader(OtherTestTranslations, delay);
                manager.SetTranslationProcessor((key, translation, loader) =>
                {
                    if (loader == firstLoader) return $"loader1:{key}:{translation}";
                    if (loader == otherLoader) return $"loader2:{key}:{translation}";
                    return "";
                });
                manager.AddLoader(firstLoader);
                manager.AddLoader(otherLoader);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("loader1:key1:value1", manager.GetTranslation("key1"));
                Assert.AreEqual("loader2:otherKey1:otherValue1", manager.GetTranslation("otherKey1"));
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestTranslationProcessorOnMissingTranslation(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var firstLoader = new TestTranslationLoader(TestTranslations, delay);
                manager.SetTranslationProcessor((key, translation, loader) =>
                {
                    if (loader == null) return $"processor-missing:{key}";
                    return translation;
                });
                manager.AddLoader(firstLoader);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("processor-missing:missingId", manager.GetTranslation("missingId"));
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfTranslationProcessorThrowingExceptionDoesntCrashManager(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var firstLoader = new TestTranslationLoader(TestTranslations, delay);
                manager.SetTranslationProcessor((key, translation, loader) => throw new ArgumentException());
                manager.AddLoader(firstLoader);
                manager.Load();
                await manager.WaitForLoad();
                manager.GetTranslation("id1");
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfTranslationProcessorThrowingExceptionDoesntCrashManagerForMissingTranslation(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var firstLoader = new TestTranslationLoader(TestTranslations, delay);
                manager.SetTranslationProcessor((key, translation, loader) => throw new ArgumentException());
                manager.AddLoader(firstLoader);
                manager.Load();
                await manager.WaitForLoad();
                manager.GetTranslation("missing_id");
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfAddingLoaderDuringLoadDoesntBlockManager(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var firstLoader = new TestTranslationLoader(TestTranslations, delay);
                var secondLoader = new TestTranslationLoader(OtherTestTranslations, delay);
                manager.AddLoader(firstLoader);
                manager.Load();
                manager.AddLoader(secondLoader);
                await manager.WaitForLoad().Timeout(TimeSpan.FromSeconds(1));
            });
        
        [TestCase(0f, ExpectedResult = null)]
        [TestCase(0.1f, ExpectedResult = null)]
        public IEnumerator TestIfLoadCanBeInvokedTwice(float delay)
            => UniTask.ToCoroutine(async () =>
            {
                var firstLoader = new TestTranslationLoader(TestTranslations, delay);
                var secondLoader = new TestTranslationLoader(OtherTestTranslations, delay);
                manager.AddLoader(firstLoader);
                manager.Load();
                manager.AddLoader(secondLoader);
                manager.Load();
                await manager.WaitForLoad().Timeout(TimeSpan.FromSeconds(1));

                Assert.AreEqual("value1", manager.GetTranslation("key1"));
                Assert.AreEqual("otherValue1", manager.GetTranslation("otherKey1"));
            });
        
        [UnityTest]
        public IEnumerator TestHasTranslation()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.IsTrue(manager.HasTranslation("key1"));
                Assert.IsTrue(manager.HasTranslation("otherKey1"));
                Assert.IsFalse(manager.HasTranslation("missingKey"));
            });

        [UnityTest]
        public IEnumerator TestIfUpdateIsInvokedAfterTranslationsAreLoaded()
            => UniTask.ToCoroutine(async () =>
            {
                var updateCount = 0;
                manager.Updated += () => { ++updateCount; };
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual(1, updateCount);
            });
        
        [UnityTest]
        public IEnumerator TestIfUpdateIsInvokedAfterEachTimeTranslationsAreLoaded()
            => UniTask.ToCoroutine(async () =>
            {
                var updateCount = 0;
                manager.Updated += () => { ++updateCount; };
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual(2, updateCount);
            });
        
        [UnityTest]
        public IEnumerator TestIfUpdateIsInvokedAfterTranslationProcessorIsChanged()
            => UniTask.ToCoroutine(async () =>
            {
                var updateCount = 0;
                manager.Updated += () => { ++updateCount; };
                manager.SetTranslationProcessor((key, translation, loader) => translation);
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                manager.AddLoader(loader);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual(2, updateCount);
            });

        [UnityTest]
        public IEnumerator TestDefaultPriority()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader)
                    .WithPriority(0);
                manager.AddLoader(loader2)
                    .WithPriority(0);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("value3", manager.GetTranslation("common"));
            });
        
        [UnityTest]
        public IEnumerator TestNormalPriority()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader)
                    .WithPriority(1);
                manager.AddLoader(loader2)
                    .WithPriority(2);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("value3", manager.GetTranslation("common"));
            });
        
        [UnityTest]
        public IEnumerator TestReversedPriority()
            => UniTask.ToCoroutine(async () =>
            {
                var loader = new TestTranslationLoader(TestTranslations, 0f);
                var loader2 = new TestTranslationLoader(OtherTestTranslations, 0f);
                manager.AddLoader(loader)
                    .WithPriority(2);
                manager.AddLoader(loader2)
                    .WithPriority(1);
                manager.Load();
                await manager.WaitForLoad();
                
                Assert.AreEqual("otherValue3", manager.GetTranslation("common"));
            });
        
        [UnityTest]
        [TestCase(0, 0, true, 0, 1, ExpectedResult = null)]
        [TestCase( 0, 1, false, 1, 1, ExpectedResult = null)]
        [TestCase( 1, 1, true, 1, 2, ExpectedResult = null)]
        [TestCase(2, 1, true, 1, 2, ExpectedResult = null)]
        [TestCase(1, 2, false, 2, 2, ExpectedResult = null)]
        [TestCase(3, 3, true, 3, 4, ExpectedResult = null)]
        public IEnumerator CheckRetries(
            int maxRetries, int requiredRetries, 
            bool expectedSuccess, int expectedErrors, int expectedLoads)
            => UniTask.ToCoroutine(async () =>
            {
                var errorCount = 0;
                manager.SetLoaderErrorHandler((loader, exception) =>
                {
                    ++errorCount;
                });
                var loader = new RetryTranslationLoader(TestTranslations, 0.1f, requiredRetries);
                manager.AddLoader(loader)
                    .WithRetries(maxRetries);
                manager.Load();
                await manager.WaitForLoad().Timeout(TimeSpan.FromSeconds(5f));

                Assert.AreEqual(expectedErrors, errorCount);
                Assert.AreEqual(expectedLoads, loader.LoadCount);
                Assert.AreEqual(expectedSuccess, manager.HasTranslation("key1"));
            });
        
    }
}