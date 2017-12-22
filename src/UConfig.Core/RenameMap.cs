namespace UConfig.Core
{
    using System;
    using System.Collections.Generic;

    public class RenameMap
    {
        private string oldName;
        internal Dictionary<string, string> mapping = new Dictionary<string, string>();

        public RenameMap RenameFrom(string fromName)
        {
            oldName = fromName;
            return this;
        }

        public RenameMap To(string renamedBy)
        {
            if (string.IsNullOrWhiteSpace(oldName))
            {
                throw new InvalidOperationException($"Call {nameof(RenameFrom)} first.");
            }

            mapping.Add(oldName, renamedBy);
            oldName = null;
            return this;
        }
    }
}