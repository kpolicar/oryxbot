using System;
using SystemServiceContainer = System.ComponentModel.Design.ServiceContainer;

namespace OryxBot.Shared.Design
{
    public class ServiceContainer : SystemServiceContainer
    {
        public T GetService<T>() {
            return ((T) base.GetService(typeof(T)))!;
        }

        public void ReplaceService(Type serviceType, object serviceInstance) {
            RemoveService(serviceType);
            AddService(serviceType, serviceInstance);
        }

        public void ReplaceService<T>(T serviceType) where T : notnull {
            ReplaceService(typeof(T), serviceType);
        }
    }
}
