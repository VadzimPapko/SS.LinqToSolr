using System;
using System.Collections.Generic;

namespace SS.LinqToSolr.Models
{
    public class Document
    {
        public virtual Dictionary<string, object> Fields => new Dictionary<string, object>();
        public virtual string this[string key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                return Fields[key].ToString();
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                Fields[key] = value;
            }
        }
    }
}
