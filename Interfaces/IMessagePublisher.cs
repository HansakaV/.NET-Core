using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.API.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string messageType , string payload, CancellationToken cancellationToken = default);
    }
}