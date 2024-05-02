namespace OriginalGOAP
{
    public interface IIdleStrategy
    {
        bool CanPerform { get; }
        bool Complete { get; }

        void Start();
        void Update(float deltaTime);
    }
}