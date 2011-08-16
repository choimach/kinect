namespace Kinect.Recognition.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Contains several extension methods used throughout the project
    /// </summary>
    public static class Extensions 
    {
        /// <summary>
        /// Finds the maximum element within an enumerable
        /// </summary>
        /// <typeparam name="TSource">The type of elements returned by the enumerable</typeparam>
        /// <typeparam name="TResult">The type of result that is to be returned</typeparam>
        /// <param name="enumerable">The enumerable to be examined</param>
        /// <param name="selector">A function used for obtaining a comparable value from the current element</param>
        /// <param name="comparator">A function used for comparing values of type TResult</param>
        /// <returns></returns>
        public static TSource Max<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector, Func<TResult, TResult, double> comparator )
        {
            if (selector == null)
                throw new ArgumentNullException("selector cannot be null");

            if (comparator == null)
                throw new ArgumentNullException("comparator cannot be null");

            TResult currMax = default(TResult);
            TSource currSource = default(TSource);
            bool initializedMax = false;

            foreach (TSource src in enumerable)
            {
                TResult tmpRes = selector(src);

                if (!initializedMax)
                {
                    initializedMax = true;
                    currMax = tmpRes;
                    currSource = src;
                }
                else if (comparator(tmpRes, currMax) > 0)
                {
                    currMax = tmpRes;
                    currSource = src;
                }
            }

            return currSource;
        }

        /// <summary>
        /// Processes xml nodes returned by an XPath query
        /// </summary>
        /// <param name="document">The xml document</param>
        /// <param name="xPath">The query</param>
        /// <param name="action">The action to be performed over the elements</param>
        public static void ProcessXmlNodes(this XmlDocument document, string xPath, Action<XmlNode> action)
        {
            if (xPath == null || xPath.Equals(string.Empty))
                throw new ArgumentException("xPath argument cannot be null nor empty");

            if (action == null)
                throw new ArgumentNullException("action cannot be null");

            XmlNodeList nodes = document.SelectNodes(xPath);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                    action(node);
            }
        }
    }
}
