using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Traduio
{
    public class TranslationTargetTest
    {
        private readonly Dictionary<string, string> TestTranslations = new()
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "keyWithParameter", "begin {parameter} end"},
            { "otherKeyWithParameter", "other begin {parameter} end"}
        };
        
        private readonly Dictionary<string, string> TestTranslations2 = new()
        {
            { "key1", "otherValue1" },
            { "key2", "otherValue2" }
        };

        private TestTranslationLoader loader1;
        private TestTranslationLoader loader2;
        private TranslationManager manager;
        private TestTranslationTarget target;
        
        [SetUp]
        public void Setup()
        {
            manager = new TranslationManager();
            loader1 = new TestTranslationLoader(TestTranslations, 0f);
            loader2 = new TestTranslationLoader(TestTranslations2, 0f);
            manager.AddLoader(loader1);
            TestTranslationTarget.Pointer.Setup(manager);
        }

        [TearDown]
        public void TestDown()
        {
            if(target != null) Object.Destroy(target.gameObject);
        }
        
        [UnityTest]
        public IEnumerator TestIfTranslationIfSetAfterAwake()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationAreNotSetBeforeAwake()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
               
                CollectionAssert.AreEqual(Array.Empty<string>(), target.Applies);
            });

        [UnityTest]
        public IEnumerator TestIfChangedTranslationIsApplied()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);

                manager.RemoveLoader(loader1);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                manager.RefreshTargets();
               
                CollectionAssert.AreEqual(new[] { "value1", "otherValue1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfChangedTranslationIsNotAppliedBeforeRefreshTargets()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);

                manager.RemoveLoader(loader1);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationIsNotAppliedWhenItsNotChanged()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                manager.RefreshTargets();
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationIsNotAppliedWhenTargetIsDisabled()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                target.enabled = false;

                manager.RemoveLoader(loader1);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                manager.RefreshTargets();
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationIsAppliedAfterRefreshTargetsAndEnable()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                target.enabled = false;

                manager.RemoveLoader(loader1);
                manager.AddLoader(loader2);
                manager.Load();
                await manager.WaitForLoad();
                manager.RefreshTargets();
                target.enabled = true;
               
                CollectionAssert.AreEqual(new[] { "value1", "otherValue1" }, target.Applies);
            });

        [UnityTest]
        public IEnumerator TestIfTranslationIsNotAppliedOnEveryEnable()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                target.enabled = false;
                target.enabled = true;

                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });

        [UnityTest]
        public IEnumerator TestIfChangedTranslationFromOtherManagerIsApplied()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);

                var otherManager = new TranslationManager();
                otherManager.AddLoader(loader2);
                TestTranslationTarget.Pointer.Setup(otherManager);
                otherManager.Load();
                await otherManager.WaitForLoad();
                otherManager.RefreshTargets();
               
                CollectionAssert.AreEqual(new[] { "value1", "otherValue1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfChangedTranslationFromOtherManagerIsNotAppliedBeforeRefreshTargets()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);

                var otherManager = new TranslationManager();
                otherManager.AddLoader(loader2);
                TestTranslationTarget.Pointer.Setup(otherManager);
                otherManager.Load();
                await otherManager.WaitForLoad();
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationFromOtherManagerIsNotAppliedWhenItsNotChanged()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                
                var otherManager = new TranslationManager();
                otherManager.AddLoader(loader1);
                TestTranslationTarget.Pointer.Setup(otherManager);
                otherManager.Load();
                await otherManager.WaitForLoad();
                otherManager.RefreshTargets();
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationFromOtherManagerIsNotAppliedWhenTargetIsDisabled()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                target.enabled = false;
        
                var otherManager = new TranslationManager();
                otherManager.AddLoader(loader1);
                TestTranslationTarget.Pointer.Setup(otherManager);
                otherManager.Load();
                await otherManager.WaitForLoad();
                otherManager.RefreshTargets();
               
                CollectionAssert.AreEqual(new[] { "value1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfTranslationFromOtherManagerIsAppliedAfterRefreshTargetsAndEnable()
            => UniTask.ToCoroutine(async () =>
            {
                manager.Load();
                await manager.WaitForLoad();
                target = CreateTarget("key1");
                target.gameObject.SetActive(true);
                target.enabled = false;
        
                var otherManager = new TranslationManager();
                otherManager.AddLoader(loader2);
                TestTranslationTarget.Pointer.Setup(otherManager);
                otherManager.Load();
                await otherManager.WaitForLoad();
                otherManager.RefreshTargets();
                target.enabled = true;
               
                CollectionAssert.AreEqual(new[] { "value1", "otherValue1" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfChangingRootAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.Root = new TranslationRoot
                {
                    Mode = TranslationRootMode.Translation,
                    Value = "otherKeyWithParameter"
                };
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfChangingRootAppliesTextAfterEnabled()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.enabled = false;
                target.Root = new TranslationRoot
                {
                    Mode = TranslationRootMode.Translation,
                    Value = "otherKeyWithParameter"
                };
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.enabled = true;
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfStartChangesSuppressApply()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.StartChanges();
                target.Root = new TranslationRoot
                {
                    Mode = TranslationRootMode.Translation,
                    Value = "otherKeyWithParameter"
                };
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.CommitChanges();
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfCommitChangesDoesntApplyWhenTargetIsDisabled()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.enabled = false;
                target.StartChanges();
                target.Root = new TranslationRoot
                {
                    Mode = TranslationRootMode.Translation,
                    Value = "otherKeyWithParameter"
                };
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.CommitChanges();
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.enabled = true;
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });

        [UnityTest]
        public IEnumerator TestIfEnableDoesntApplyWhenDuringChanges()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.enabled = false;
                target.StartChanges();
                target.Root = new TranslationRoot
                {
                    Mode = TranslationRootMode.Translation,
                    Value = "otherKeyWithParameter"
                };
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.enabled = true;
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.CommitChanges();
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfSettingParameterAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.SetParameter("parameter", new TranslationRoot
                {
                    Mode = TranslationRootMode.RawText,
                    Value = "xyz"
                });
                target.SetParameter("parameter", new TranslationRoot
                {
                    Mode = TranslationRootMode.RawText,
                    Value = "abc"
                });
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "begin xyz end", "begin abc end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfClearParametersAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.SetParameter("parameter", new TranslationRoot
                {
                    Mode = TranslationRootMode.RawText,
                    Value = "xyz"
                });
                target.ClearParameters();
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "begin xyz end", "begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfRemovingParametersAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.SetParameter("parameter", new TranslationRoot
                {
                    Mode = TranslationRootMode.RawText,
                    Value = "xyz"
                });
                target.RemoveParameter("parameter");
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "begin xyz end", "begin {parameter} end" }, target.Applies);
            });
        
        private async UniTask LoadTargetWithParameter()
        {
            manager.Load();
            await manager.WaitForLoad();
            target = CreateTarget("keyWithParameter");
            target.gameObject.SetActive(true);
            target.enabled = true;
        }
        
        private TestTranslationTarget CreateTarget(string initKey)
        {
            var go = new GameObject("target");
            go.SetActive(false);
            var target = go.AddComponent<TestTranslationTarget>();
            target.Root = new TranslationRoot
            {
                Mode = TranslationRootMode.Translation, 
                Value = initKey
            };
            return target;
        }
    }
}