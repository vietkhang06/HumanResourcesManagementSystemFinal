using CommunityToolkit.Mvvm.Messaging.Messages;

namespace HumanResourcesManagementSystemFinal.Messages
{
    public class ReloadRequestMessage : ValueChangedMessage<string>
    {
        public ReloadRequestMessage(string value) : base(value) { }
    }
}