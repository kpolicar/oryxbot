namespace OryxBot.Shared.Design
{
    public interface HasDependencies
    {
        public void BindDependencies(ServiceContainer serviceContainer);
    }
}
