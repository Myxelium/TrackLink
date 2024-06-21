using MediatR;

namespace api.Handlers.Hello
{
    public static class HelloHandler
    {
        public record HelloCommand(): IRequest<string>;
    
        public class Handler : IRequestHandler<HelloCommand, string>
        {
            public Task<string> Handle(HelloCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult("Hey there!");
            }
        }
    }
}