using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace HumanResourcesManagementSystemFinal.Messages
{
    public class ReloadRequestMessage : ValueChangedMessage<string>
    {
        public ReloadRequestMessage(string value) : base(value) { }
    }
}