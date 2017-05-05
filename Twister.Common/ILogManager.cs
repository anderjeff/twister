using System;

namespace Twister.Common
{
    public interface ILogManager
    {
        ILogger GetLogger(Type type);
    }
}