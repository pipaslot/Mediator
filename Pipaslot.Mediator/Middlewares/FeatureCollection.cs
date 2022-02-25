using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Pipaslot.Mediator.Middlewares
{
    public class FeatureCollection : IFeatureCollection
    {
        private IDictionary<Type, object>? _features;
        private volatile int _containerRevision;

        public virtual int Revision => _containerRevision;

        public object this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return _features != null && _features.TryGetValue(key, out var result) ? result : null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    if (_features != null && _features.Remove(key))
                    {
                        _containerRevision++;
                    }
                    return;
                }

                if (_features == null)
                {
                    _features = new Dictionary<Type, object>();
                }
                _features[key] = value;
                _containerRevision++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            if (_features != null)
            {
                foreach (var pair in _features)
                {
                    yield return pair;
                }
            }
        }

        public TFeature Get<TFeature>()
        {
            return (TFeature)this[typeof(TFeature)];
        }

        public void Set<TFeature>(TFeature instance)
        {
            this[typeof(TFeature)] = instance;
        }
    }
}
