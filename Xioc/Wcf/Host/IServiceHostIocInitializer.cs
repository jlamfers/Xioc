namespace Xioc.Wcf.Host
{
   public interface IServiceHostIocInitializer
   {
      void BindTypes(IBinder binder);
   }
}