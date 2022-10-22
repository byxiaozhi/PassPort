using PassPort.Models;

namespace PassPort.Interfaces
{
    public interface IModule
    {
        void Initialize(Node node) => throw new NotImplementedException();

        Task InitializeAsync(Node node) => throw new NotImplementedException();

        void Forward(Context ctx) => throw new NotImplementedException();

        Task ForwardAsync(Context ctx) => throw new NotImplementedException();

        void Backward(Context ctx) => throw new NotImplementedException();

        Task BackwardAsync(Context ctx) => throw new NotImplementedException();
    }
}
