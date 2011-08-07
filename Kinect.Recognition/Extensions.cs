namespace Kinect.Recognition.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public static class Extensions 
    {
        public static TSource Max<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector, Func<TResult, TResult, double> comparator )
        {
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
            XmlNodeList nodes = document.SelectNodes(xPath);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                    action(node);
            }
        }
    }
}
