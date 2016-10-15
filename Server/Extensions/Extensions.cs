﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server.Extensions
{
    public static class Extensions
    {
        public static async Task ReadStartElementAsync(this XmlReader reader, string localname, string ns)
        {
            if (await reader.MoveToContentAsync() != XmlNodeType.Element)
            {
                throw new InvalidOperationException(reader.NodeType.ToString() + " is an invalid XmlNodeType");
            }
            if ((reader.LocalName == localname) && (reader.NamespaceURI == ns))
            {
                await reader.ReadAsync();
            }
            else
            {
                throw new InvalidOperationException("localName or namespace doesn’t match");
            }
        }
        public static async Task ReadEndElementAsync(this XmlReader reader)
        {
            if (await reader.MoveToContentAsync() != XmlNodeType.EndElement)
            {
                throw new InvalidOperationException();
            }
            await reader.ReadAsync();
        }
        public static async Task<bool> ReadToNextSiblingAsync(this XmlReader reader, string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw new ArgumentException("localName is empty or null");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }

            // atomize local name and namespace
            localName = reader.NameTable.Add(localName);
            namespaceURI = reader.NameTable.Add(namespaceURI);

            // find the next sibling
            XmlNodeType nt;
            do
            {
                await reader.SkipAsync();
                if (reader.ReadState != ReadState.Interactive)
                    break;
                nt = reader.NodeType;
                if (nt == XmlNodeType.Element &&
                     ((object)localName == (object)reader.LocalName) &&
                     ((object)namespaceURI == (object)reader.NamespaceURI))
                {
                    return true;
                }
            } while (nt != XmlNodeType.EndElement && !reader.EOF);

            return false;
        }
        public static async Task<bool> ReadToFollowingAsync(this XmlReader reader, string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw new ArgumentException("localName is empty or null");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }

            // atomize local name and namespace
            localName = reader.NameTable.Add(localName);
            namespaceURI = reader.NameTable.Add(namespaceURI);

            // find element with that name
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && ((object)localName == (object)reader.LocalName) && ((object)namespaceURI == (object)reader.NamespaceURI))
                {
                    return true;
                }
            }
            return false;
        }
        public static async Task<bool> ReadToDescendantAsync(this XmlReader reader, string localName, string namespaceURI)
        {
            if (localName == null || localName.Length == 0)
            {
                throw new ArgumentException("localName is empty or null");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            // save the element or root depth
            int parentDepth = reader.Depth;
            if (reader.NodeType != XmlNodeType.Element)
            {
                // adjust the depth if we are on root node
                if (reader.ReadState == ReadState.Initial)
                {
                    parentDepth--;
                }
                else
                {
                    return false;
                }
            }
            else if (reader.IsEmptyElement)
            {
                return false;
            }

            // atomize local name and namespace
            localName = reader.NameTable.Add(localName);
            namespaceURI = reader.NameTable.Add(namespaceURI);

            // find the descendant
            while (await reader.ReadAsync() && reader.Depth > parentDepth)
            {
                if (reader.NodeType == XmlNodeType.Element && ((object)localName == (object)reader.LocalName) && ((object)namespaceURI == (object)reader.NamespaceURI))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
