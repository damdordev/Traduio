using NUnit.Framework;

namespace Traduio
{
    public class TranslationManagerPointerTest
    {

        [Test]
        public void TestEmptyPointer()
        {
            var pointer = new TranslationManagerPointer();

            Assert.AreEqual(null, pointer.Manager);
        }
        
        [Test]
        public void TestIfChangingManagerInvokesChanged()
        {
            var changedCount = 0;
            var pointer = new TranslationManagerPointer();
            pointer.Changed += () => ++changedCount; 
            pointer.Setup(new TranslationManager());

            Assert.AreEqual(1, changedCount);
        }
        
        [Test]
        public void TestIfChangingManagerChangesReference()
        {
            var pointer = new TranslationManagerPointer();
            var manager = new TranslationManager();
            pointer.Setup(manager);

            Assert.AreEqual(manager, pointer.Manager);
        }
        
        [Test]
        public void TestIfChangingManagerToTheSameDoesntChangeReference()
        {
            var pointer = new TranslationManagerPointer();
            var manager = new TranslationManager();
            pointer.Setup(manager);
            pointer.Setup(manager);

            Assert.AreEqual(manager, pointer.Manager);
        }
        
        [Test]
        public void TestIfChangingManagerToTheSameDoesntInvokeChanged()
        {
            var pointer = new TranslationManagerPointer();
            var manager = new TranslationManager();
            pointer.Setup(manager);
            
            var changedCount = 0;
            pointer.Changed += () => ++changedCount; 
            pointer.Setup(manager);

            Assert.AreEqual(0, changedCount);
        }
        
    }
}