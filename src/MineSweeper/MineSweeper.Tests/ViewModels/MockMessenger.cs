using CommunityToolkit.Mvvm.Messaging;

namespace MineSweeper.Tests.ViewModels;

internal sealed class MockMessenger : IMessenger
{
    public List<object> SentMessages { get; } = [];

    public void Send<TMessage>(TMessage message) where TMessage : class
    {
        SentMessages.Add(message!);
    }

    bool IMessenger.IsRegistered<TMessage, TToken>(object recipient, TToken token)
    {
        return false;
    }

    void IMessenger.Register<TRecipient, TMessage, TToken>(TRecipient recipient, TToken token,
        MessageHandler<TRecipient, TMessage> handler)
    {
        // No-op for mock
    }

    void IMessenger.UnregisterAll(object recipient)
    {
        // No-op for mock
    }

    void IMessenger.UnregisterAll<TToken>(object recipient, TToken token)
    {
        // No-op for mock
    }

    void IMessenger.Unregister<TMessage, TToken>(object recipient, TToken token)
    {
        // No-op for mock
    }

    TMessage IMessenger.Send<TMessage, TToken>(TMessage message, TToken token)
    {
        SentMessages.Add(message!);
        return message;
    }

    void IMessenger.Cleanup()
    {
        // No-op for mock
    }

    public void Reset()
    {
        SentMessages.Clear();
    }
}