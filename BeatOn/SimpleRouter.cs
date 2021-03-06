﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BeatOn
{
    public class SimpleRouter
    {
        private Dictionary<string, RouteTreeNode<IHandleRequest>> _hostMethodRoutes = new Dictionary<string, RouteTreeNode<IHandleRequest>>();
        public void AddRoute(string method, string path, IHandleRequest action)
        {
            var split = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new ArgumentException("Path cannot be empty");
            var paths = new Stack<string>(split);

            method = method.ToUpper();

            RouteTreeNode<IHandleRequest> node;

            //always have an empty root node
            if (!_hostMethodRoutes.ContainsKey(method))
            {
                node = new RouteTreeNode<IHandleRequest>(null);
                _hostMethodRoutes.Add(method, node);
            }
            else
            {
                node = _hostMethodRoutes[method];
            }

            if (!node.TryAdd(paths, action))
                throw new ArgumentException("Route already exists");
        }

        public IHandleRequest FindRoute(string method, string path)
        {
            var split = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new ArgumentException("Path cannot be empty");
            var paths = new Stack<string>(split);

            method = method.ToUpper();
            if (!_hostMethodRoutes.ContainsKey(method))
                return null;

            return _hostMethodRoutes[method].FindNode(paths)?.Value;
        }

        class RouteTreeNode<T> where T : class
        {
            public RouteTreeNode(T value)
            {
                Value = value;
            }

            public T Value { get; set; }

            public Dictionary<string, RouteTreeNode<T>> Nodes { get; } = new Dictionary<string, RouteTreeNode<T>>();

            public RouteTreeNode<T> FindNode(Stack<string> paths)
            {
                if (paths.Count == 0)
                    return this;

                var nextSegment = paths.Pop();

                if (!Nodes.ContainsKey(nextSegment))
                    return null;

                return Nodes[nextSegment].FindNode(paths);
            }

            public bool TryAdd(Stack<string> paths, T value)
            {
                var path = paths.Pop();
                RouteTreeNode<T> node = null;
                if (Nodes.ContainsKey(path))
                {
                    if (paths.Count == 0)
                    {
                        if (Nodes[path].Value != null)
                        {
                            return false;
                        }
                        else
                        {
                            Nodes[path].Value = value;
                            return true;
                        }
                    }
                    else
                    {
                        node = Nodes[path];
                    }
                }
                else
                {
                    node = new RouteTreeNode<T>(null);
                    Nodes.Add(path, node);
                    if (paths.Count == 0)
                    {
                        node.Value = value;
                        return true;
                    }
                }
                                
                return node.TryAdd(paths, value);                
            }
        }
    }
}