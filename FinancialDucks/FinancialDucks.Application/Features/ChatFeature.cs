using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class ChatFeature
    { 
        public record ChatRequest(string Prompt) : IRequest<string>;

        public class ChatRequestHandler : IRequestHandler<ChatRequest, string>
        {
            private IPromptEngine _promptEngine;

            public ChatRequestHandler(IPromptEngine promptEngine)
            {
                _promptEngine = promptEngine;
            }

            public async Task<string> Handle(ChatRequest request, CancellationToken cancellationToken)
            {
                return await _promptEngine.Chat(request.Prompt);
            }
        }
    }
}
