using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Damdor.Traduio.Tests
{
    public class TranslationTargetTest
    {
        private TestTranslationLoader loader;
        private TranslationManager manager;
        private TestTranslationTarget target;
        
        [SetUp]
        public void Setup()
        {
            manager = new TranslationManager();
            loader = new TestTranslationLoader(new Dictionary<string, string>(), 0f);
            manager.AddLoader(loader);
            TestTranslationTarget.Pointer.Setup(manager);
            var go = new GameObject("target");
            go.SetActive(false);
            target = go.AddComponent<TestTranslationTarget>();
        }

        [TearDown]
        public void TearDown()
        {
            if(target != null) Object.Destroy(target.gameObject);
        }
        
        [UnityTest]
        public IEnumerator Test_NotAppliedBeforeAwake()
            => UniTask.ToCoroutine(async () =>
            {
                Assert.AreEqual(0, target.Applies);
            });

        [UnityTest]
        public IEnumerator Test_AppliedAfterAwake()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_AppliedAfterRefreshWhenEnabled()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
                target.Refresh();

                Assert.AreEqual(2, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_NotAppliedAfterRefreshWhenDisabled()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);

                target.enabled = false;
                target.Refresh();

                Assert.AreEqual(1, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_AppliedAfterRefreshAndEnable()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);

                target.enabled = false;
                target.Refresh();
                target.enabled = true;

                Assert.AreEqual(2, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_AppliedManagerChanged()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);

                var newManager = new TranslationManager();
                TestTranslationTarget.Pointer.Setup(newManager);
                Assert.AreEqual(1, target.Applies);
                newManager.RefreshTargets();

                Assert.AreEqual(2, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_NotAppliedManagerChangedAndDisabled()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);

                target.enabled = false;
                var newManager = new TranslationManager();
                TestTranslationTarget.Pointer.Setup(newManager);
                newManager.RefreshTargets();

                Assert.AreEqual(1, target.Applies);
            });

        [UnityTest]
        public IEnumerator Test_AppliedManagerRefresh()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                
                Assert.AreEqual(1, target.Applies);
                
                manager.RefreshTargets();
                
                Assert.AreEqual(2, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_NotAppliedManagerRefreshAndDisabled()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                
                Assert.AreEqual(1, target.Applies);
                
                target.enabled = false;
                manager.RefreshTargets();

                Assert.AreEqual(1, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_MultipleRefresh()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
                
                target.Refresh();
                target.Refresh();
                target.Refresh();

                Assert.AreEqual(4, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_MultipleRefreshBatch()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
                
                target.StartChanges();
                target.Refresh();
                target.Refresh();
                target.Refresh();

                Assert.AreEqual(1, target.Applies);
                
                target.CommitChanges();
                Assert.AreEqual(2, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_NotAppliedOnEveryEnable()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);

                target.enabled = false;
                target.enabled = true;

                Assert.AreEqual(1, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_NotAppliedOnEnableWhenDuringChange()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
                
                target.StartChanges();
                target.enabled = false;
                target.enabled = true;
                target.Refresh();

                Assert.AreEqual(1, target.Applies);
                
                target.CommitChanges();
                Assert.AreEqual(2, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_NotAppliedNoChangeDuringChanging()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
                
                target.StartChanges();
                target.CommitChanges();

                Assert.AreEqual(1, target.Applies);
            });
        
        [UnityTest]
        public IEnumerator Test_MultiLevelChanging()
            => UniTask.ToCoroutine(async () =>
            {
                target.gameObject.SetActive(true);
                Assert.AreEqual(1, target.Applies);
                
                target.StartChanges();
                target.StartChanges();
                target.Refresh();
                target.CommitChanges();
                Assert.AreEqual(1, target.Applies);
                target.CommitChanges();

                Assert.AreEqual(2, target.Applies);
            });
        
    }
}