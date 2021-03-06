﻿using System.Collections.Generic;

namespace CondenserDotNet.Core.Routing
{
    public class RandomRoutingStrategy<T> : IRoutingStrategy<T>
    {
        public T RouteTo(List<T> instances) => instances?.Count > 0 ? instances[RandHelper.Next(0, instances.Count)] : default(T);
        public string Name { get; } = RouteStrategy.Random.ToString();
    }
}