﻿using System.ComponentModel;

namespace UmbracoExamine.Core
{
    public class IndexingNodeEventArgs : CancelEventArgs 
    {
        public IndexingNodeEventArgs(int nodeId)
        {
            NodeId = nodeId;
        }

        public int NodeId { get; private set; }
    }
}