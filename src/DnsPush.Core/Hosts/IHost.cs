using System;
using System.Threading;
using System.Threading.Tasks;

namespace DnsPush.Core.Hosts
{
    public interface IHost<TOptions, TUpdateRecordOptions>
        where TOptions : new()
        where TUpdateRecordOptions : new()
    {
        string Key { get; }
        string DisplayName { get; }
        Task<UpdateRecordResult> UpdateRecordAsync(TUpdateRecordOptions options, CancellationToken cancellationToken);
    }
}