using System;
using System.Collections.Generic;

namespace Lamar.Testing.Bugs.MultiThreadingProblem
{
    public class ImportMultiple1
    {
        protected static int counter;

        public ImportMultiple1(
            IList<ISimpleAdapter> adapters)
        {
            if (adapters == null)
            {
                throw new ArgumentNullException(nameof(adapters));
            }

            int adapterCount = 0;
            foreach (var adapter in adapters)
            {
                if (adapter == null)
                {
                    throw new ArgumentException("adapters item should be not null");
                }

                ++adapterCount;
            }

            if (adapterCount != 5)
            {
                throw new ArgumentException("there should be 5 adapters and there where: " + adapterCount, nameof(adapters));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        protected ImportMultiple1()
        {
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }

    public class ImportMultiple2
    {
        protected static int counter;

        public ImportMultiple2(
            ISimpleAdapter[] adapters)
        {
            if (adapters == null)
            {
                throw new ArgumentNullException(nameof(adapters));
            }

            int adapterCount = 0;
            foreach (var adapter in adapters)
            {
                if (adapter == null)
                {
                    throw new ArgumentException("adapters item should be not null");
                }

                ++adapterCount;
            }

            if (adapterCount != 5)
            {
                throw new ArgumentException("there should be 5 adapters and there where: " + adapterCount, nameof(adapters));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        protected ImportMultiple2()
        {
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }

    public class ImportMultiple3
    {
        protected static int counter;

        public ImportMultiple3(
            ISimpleAdapter[] adapters)
        {
            if (adapters == null)
            {
                throw new ArgumentNullException(nameof(adapters));
            }

            int adapterCount = 0;
            foreach (var adapter in adapters)
            {
                if (adapter == null)
                {
                    throw new ArgumentException("adapters item should be not null");
                }

                ++adapterCount;
            }

            if (adapterCount != 5)
            {
                throw new ArgumentException("there should be 5 adapters and there where: " + adapterCount, nameof(adapters));
            }

            System.Threading.Interlocked.Increment(ref counter);
        }

        protected ImportMultiple3()
        {
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }
    }
}
