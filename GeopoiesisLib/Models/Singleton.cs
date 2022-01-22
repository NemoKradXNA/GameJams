using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class Singleton<T> where T : class, new()
    {
        protected static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();

                return _instance;
            }
        }
    }
}
