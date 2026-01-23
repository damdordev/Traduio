using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Damdor.Traduio.Tests
{
    public class BaseSmartTranslationTest
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
        private TestBaseSmartTranslation target;
        
        [SetUp]
        public void Setup()
        {
            manager = new TranslationManager();
            loader1 = new TestTranslationLoader(TestTranslations, 0f);
            loader2 = new TestTranslationLoader(TestTranslations2, 0f);
            manager.AddLoader(loader1);
            TestBaseSmartTranslation.Pointer.Setup(manager);
        }

        [TearDown]
        public void TestDown()
        {
            if(target != null) Object.Destroy(target.gameObject);
        }

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
        public IEnumerator TestIfChangingRootAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.Key = SmartTranslationKey.Translation("otherKeyWithParameter");
                
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfChangingRootAppliesTextAfterEnabled()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.enabled = false;
                target.Key = SmartTranslationKey.Translation("otherKeyWithParameter");
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end" }, target.Applies);
                target.enabled = true;
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "other begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfSettingParameterAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.SetParameter("parameter", SmartTranslationKey.Raw("xyz"));
                target.SetParameter("parameter", SmartTranslationKey.Raw("abc"));
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "begin xyz end", "begin abc end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfClearParametersAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.SetParameter("parameter", SmartTranslationKey.Raw("xyz"));
                target.ClearParameters();
               
                CollectionAssert.AreEqual(new[] { "begin {parameter} end", "begin xyz end", "begin {parameter} end" }, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator TestIfRemovingParametersAppliesText()
            => UniTask.ToCoroutine(async () =>
            {
                await LoadTargetWithParameter();
                target.SetParameter("parameter", SmartTranslationKey.Raw("xyz"));
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
        
        private TestBaseSmartTranslation CreateTarget(string initKey)
        {
            var go = new GameObject("target");
            go.SetActive(false);
            var target = go.AddComponent<TestBaseSmartTranslation>();
            target.Key = new SmartTranslationKey
            {
                Mode = SmartTranslationKeyMode.Translation, 
                Value = initKey
            };
            return target;
        }
    }
}