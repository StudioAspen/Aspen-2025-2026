// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Text;

namespace Animancer
{
    /// <summary>An <see cref="AnimancerNode"/> and its <see cref="StartingWeight"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/NodeWeight
    public readonly struct NodeWeight
    {
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerNode"/>.</summary>
        public readonly AnimancerNode Node;

        /// <summary>The <see cref="AnimancerNode.Weight"/> from when this struct was captured.</summary>
        public readonly float StartingWeight;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="NodeWeight"/>.</summary>
        public NodeWeight(AnimancerNode node)
        {
            Node = node;
            StartingWeight = node.Weight;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="NodeWeight"/>.</summary>
        public NodeWeight(AnimancerNode node, float startingWeight)
        {
            Node = node;
            StartingWeight = startingWeight;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a copy of `copyFrom`.</summary>
        public NodeWeight(NodeWeight copyFrom, CloneContext context)
        {
            Node = context.GetOrCreateCloneOrOriginal(copyFrom.Node);
            StartingWeight = copyFrom.StartingWeight;
        }

        /************************************************************************************************************************/

        /// <summary>Appends a detailed descrption of this object.</summary>
        public void AppendDescription(StringBuilder text, float targetWeight)
        {
            if (Node == null)
            {
                text.Append("Null: ")
                    .Append(StartingWeight)
                    .Append(" -> ")
                    .Append(targetWeight);
            }
            else
            {
                text.Append(Node.GetPath())
                    .Append(": ")
                    .Append(StartingWeight)
                    .Append(" -> ")
                    .Append(Node.Weight)
                    .Append(" -> ")
                    .Append(targetWeight);
            }
        }

        /************************************************************************************************************************/
    }
}

