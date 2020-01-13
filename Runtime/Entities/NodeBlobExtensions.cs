using System.Collections.Generic;
using System.Linq;
using EntitiesBT.Core;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace EntitiesBT.Entities
{
    public static class NodeBlobExtensions
    {
        public static BlobAssetReference<NodeBlob> ToBlob(this INodeDataBuilder root, Allocator allocator = Allocator.Persistent)
        {
            return root.Flatten(builder => builder.Children, builder => builder.Self).ToArray().ToBlob(allocator);
        }

        public static unsafe BlobAssetReference<NodeBlob> ToBlob(this ITreeNode<INodeDataBuilder>[] nodes, Allocator allocator)
        {
            var blobSize = nodes.Select(n => n.Value.Size).Sum();
            var size = NodeBlob.CalculateSize(nodes.Length, blobSize);
            using (var blobBuilder = new BlobBuilder(Allocator.Temp, size))
            {
                ref var blob = ref blobBuilder.ConstructRoot<NodeBlob>();
                var types = blobBuilder.Allocate(ref blob.Types, nodes.Length);
                var offsets = blobBuilder.Allocate(ref blob.Offsets, nodes.Length);
                var unsafeDataPtr = (byte*) blobBuilder.Allocate(ref blob.DefaultDataBlob, blobSize).GetUnsafePtr();

                var offset = 0;
                for (var i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    types[i] = node.Value.NodeId;
                    offsets[i] = offset;
                    node.Value.Build(unsafeDataPtr + offset, nodes);
                    offset += node.Value.Size;
                }
                
                var endIndices = blobBuilder.Allocate(ref blob.EndIndices, nodes.Length);
                // make sure the memory is clear to 0 (even it had been cleared on allocate)
                UnsafeUtility.MemSet(endIndices.GetUnsafePtr(), 0, sizeof(int) * endIndices.Length);
                for (var i = nodes.Length - 1; i >= 0; i--)
                {
                    var endIndex = i + 1;
                    var node = nodes[i];
                    while (node != null && endIndices[node.Index] == 0)
                    {
                        endIndices[node.Index] = endIndex;
                        node = node.Parent;
                    }
                }
                
                var states = blobBuilder.Allocate(ref blob.States, nodes.Length);
                UnsafeUtility.MemClear(states.GetUnsafePtr(), sizeof(NodeState) * states.Length);
                
                var runtimeDataBlob = blobBuilder.Allocate(ref blob.RuntimeDataBlob, blobSize);
                UnsafeUtility.MemCpy(runtimeDataBlob.GetUnsafePtr(), unsafeDataPtr, blobSize);
                
                return blobBuilder.CreateBlobAssetReference<NodeBlob>(allocator);
            }
        }
    }
}
