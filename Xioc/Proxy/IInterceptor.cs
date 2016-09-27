namespace Xioc.Proxy
{
    public interface IInterceptor
    {
        void Intercept(IInvocation invocation);
    }
}
