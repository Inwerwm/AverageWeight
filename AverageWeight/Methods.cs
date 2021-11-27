using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using System;
using System.Collections.Generic;

namespace AverageWeight
{
    static class Methods
    {
        public static void Update(IPEConnector connector, IPXPmx pmx, PmxUpdateObject option = PmxUpdateObject.All, int index = -1)
        {
            connector.Pmx.Update(pmx, option, index);
            connector.Form.UpdateList(ConvUObjrct_DtoX(option));
            connector.View.PmxView.UpdateModel();
        }

        public static PEPlugin.Pmd.UpdateObject ConvUObjrct_DtoX(PmxUpdateObject input)
        {
            PEPlugin.Pmd.UpdateObject output;
            switch (input)
            {
                case PmxUpdateObject.None:
                    output = PEPlugin.Pmd.UpdateObject.None;
                    break;
                case PmxUpdateObject.All:
                    output = PEPlugin.Pmd.UpdateObject.All;
                    break;
                case PmxUpdateObject.Header:
                    output = PEPlugin.Pmd.UpdateObject.Header;
                    break;
                case PmxUpdateObject.ModelInfo:
                    output = PEPlugin.Pmd.UpdateObject.All;
                    break;
                case PmxUpdateObject.Vertex:
                    output = PEPlugin.Pmd.UpdateObject.Vertex;
                    break;
                case PmxUpdateObject.Face:
                    output = PEPlugin.Pmd.UpdateObject.Face;
                    break;
                case PmxUpdateObject.Material:
                    output = PEPlugin.Pmd.UpdateObject.Material;
                    break;
                case PmxUpdateObject.Bone:
                    output = PEPlugin.Pmd.UpdateObject.Bone;
                    break;
                case PmxUpdateObject.Morph:
                    output = PEPlugin.Pmd.UpdateObject.Morph;
                    break;
                case PmxUpdateObject.Node:
                    output = PEPlugin.Pmd.UpdateObject.Node;
                    break;
                case PmxUpdateObject.Body:
                    output = PEPlugin.Pmd.UpdateObject.Body;
                    break;
                case PmxUpdateObject.Joint:
                    output = PEPlugin.Pmd.UpdateObject.Joint;
                    break;
                case PmxUpdateObject.SoftBody:
                    output = PEPlugin.Pmd.UpdateObject.All;
                    break;
                default:
                    output = PEPlugin.Pmd.UpdateObject.All;
                    break;
            }

            return output;
        }

        public static double V3SquareDistance(V3 a, V3 b)
        {
            return Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2);
        }

        public static double V3Distance(V3 a, V3 b)
        {
            return Math.Sqrt(V3SquareDistance(a, b));
        }
    }
}

public static partial class TupleEnumerable
{
    public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        IEnumerable<(T item, int index)> impl()
        {
            var i = 0;
            foreach (var item in source)
            {
                yield return (item, i);
                ++i;
            }
        }

        return impl();
    }
}

