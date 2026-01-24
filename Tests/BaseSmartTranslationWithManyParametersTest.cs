using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Damdor.Traduio.Tests
{
    public class BaseSmartTranslationWithManyParametersTest
    {
        public class TestNumberTranslationTarget : BaseNumberTranslationTarget
        {
            protected override string Convert(float x)
            {
                return TraduioMainManager.Pointer.Manager.GetTranslation("number").Replace("{x}", x.ToString(CultureInfo.InvariantCulture));
            }
        }

        public class TestSmartTranslation : BaseSmartTranslation
        {
            public List<string> Applies = new();

            protected override void Apply(string text)
            {
                Applies.Add(text);
            }
        }

        private GameObject go;
        private TestSmartTranslation smartTranslation;

        [SetUp]
        public void Setup()
        {
            go = new GameObject();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }
        
        [UnityTest]
        public IEnumerator TestIfTranslationIsAppliedOnlyOnceWhenAllParametersChanged()
            => UniTask.ToCoroutine(async () =>
            {
                CreateSmartTranslation("xx {value1} {value2}");
                CreateTarget(5, "value1");
                CreateTarget(10, "value2");

                await CreateManager(new Dictionary<string, string>
                {
                    { "number", "number: {x}" }
                });

                CollectionAssert.AreEqual(
                    new[] { "xx number: 5 number: 10" },
                    smartTranslation.Applies
                );
                
                await CreateManager(new Dictionary<string, string>
                {
                    { "number", "other number: {x}" }
                });
                
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "xx number: 5 number: 10", 
                        "xx other number: 5 other number: 10"
                    },
                    smartTranslation.Applies
                );
            });

        private void CreateSmartTranslation(string key)
        {
            smartTranslation = go.AddComponent<TestSmartTranslation>();
            smartTranslation.Key = SmartTranslationKey.Raw(key);
        }
        
        private void CreateTarget(int value, string key)
        {
            var parameter = go.AddComponent<TestNumberTranslationTarget>();
            parameter.Value = value;
            parameter.Target = smartTranslation;
            parameter.Key = key;
        }

        private async UniTask CreateManager(Dictionary<string, string> translations)
        {
            var manager = new TranslationManager();
            manager.AddLoader(new DictionaryTranslationLoader(translations));
            manager.Load();
            await manager.WaitForLoad();
            TraduioMainManager.Pointer.Setup(manager);
            manager.RefreshTargets();
        }
    }
}