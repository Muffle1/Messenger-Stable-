namespace Messenger
{
    public interface IEventSubscriber
    {
        public void SubscribeEvent();
        public void UnsubscribeFromEvent();
    }
}
