using System;

namespace dnspush.Hosts
{
    public interface IHost<TOptions, TSetRecordOptions>
        where TOptions : new()
        where TSetRecordOptions : new()
    {
        string Key { get; }
        string DisplayName { get; }
    }
}