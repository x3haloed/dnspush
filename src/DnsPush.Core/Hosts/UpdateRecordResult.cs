using System;
using System.Collections.Generic;
using System.Linq;

namespace DnsPush.Core.Hosts
{
    public class UpdateRecordResult
    {
        public UpdateRecordResult(bool success, IEnumerable<string> errors)
        {
            Success = success;
            Errors = errors == null ? Array.Empty<string>() : errors.ToArray();
        }

        public bool Success { get; }
        public string[] Errors { get; }
    }
}