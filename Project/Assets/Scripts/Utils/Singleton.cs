namespace PokerCats
{
    public class Singleton<T> where T : class
    {
        private static T _instance;
        public static T Instance { get { return _instance; } }

        public Singleton()
        {
            _instance = this as T;
        }

        public virtual void Shutdown()
        {
            _instance = null;
        }
    }
}