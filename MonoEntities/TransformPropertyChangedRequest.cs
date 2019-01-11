using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoEntities.Components;

namespace MonoEntities
{
    internal struct TransformPropertyChangedRequest
    {
        internal Transform2DComponent Sender { get; }

        internal PropertyChangedExtendedEventArgs Arguments { get; }

        internal TransformPropertyChangedRequest(Transform2DComponent sender, PropertyChangedExtendedEventArgs args)
        {
            Sender = sender;
            Arguments = args;
        }
    }
}
