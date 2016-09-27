namespace Xioc.Wcf
{
   public interface IServiceClient
   {
      object Target { get; }
      IServiceClient Initialize(object target);
   }
}