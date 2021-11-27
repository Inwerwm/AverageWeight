using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;

namespace AverageWeight
{
    public class AverageWeight : PEPluginClass
    {
        public AverageWeight() : base()
        {
        }

        public override string Name
        {
            get
            {
                return "選択頂点のウェイトを周辺頂点から設定";
            }
        }

        public override string Version
        {
            get
            {
                return "1.0";
            }
        }

        public override string Description
        {
            get
            {
                return "周辺頂点のウェイトから距離を考慮して選択頂点のウェイトを設定する";
            }
        }

        public override IPEPluginOption Option
        {
            get
            {
                // boot時実行, プラグインメニューへの登録, メニュー登録名
                return new PEPluginOption(false, true, "選択頂点のウェイトを周辺頂点から設定");
            }
        }

        public override void Run(IPERunArgs args)
        {
            try
            {
                var pmx = args.Host.Connector.Pmx.GetCurrentState();

                // 選択頂点を読み込み
                var selectedVertexIndices = args.Host.Connector.View.PmxView.GetSelectedVertexIndices();
                // 各頂点を含む面を抽出
                List<List<IPXFace>> selectedVerticesIncludedFacesLists = selectedVertexIndices.Select(i => GetIncludedFacesOf(pmx.Vertex[i], pmx)).ToList();

                // 各頂点の一次近接点
                List<List<IPXVertex>> neighbors = new List<List<IPXVertex>>();
                for (int i = 0; i < selectedVerticesIncludedFacesLists.Count; i++)
                {
                    List<IPXVertex> n = new List<IPXVertex>();
                    foreach (IPXFace f in selectedVerticesIncludedFacesLists[i])
                    {
                        if (f.Vertex1 == pmx.Vertex[selectedVertexIndices[i]])
                        {
                            n.Add(f.Vertex2);
                            n.Add(f.Vertex3);
                        }
                        else
                        {
                            n.Add(f.Vertex1);
                            if (f.Vertex2 == pmx.Vertex[selectedVertexIndices[i]])
                                n.Add(f.Vertex3);
                            else
                                n.Add(f.Vertex2);
                        }
                    }
                    neighbors.Add(n);
                }

                var weightsByBone = new Dictionary<IPXBone, float>();
                for (int count = 0; count < neighbors.Count; count++)
                {
                    List<IPXVertex> n = neighbors[count];
                    List<float> nInvDistance = new List<float>();
                    foreach (var p in n)
                    {
                        float invDistance = (float)(1.0 / Methods.V3Distance(pmx.Vertex[selectedVertexIndices[count]].Position, p.Position));
                        // ボーンごとにウェイトを積算
                        setWeight(p, 1, invDistance, ref weightsByBone);
                        setWeight(p, 2, invDistance, ref weightsByBone);
                        setWeight(p, 3, invDistance, ref weightsByBone);
                        setWeight(p, 4, invDistance, ref weightsByBone);

                        nInvDistance.Add(invDistance);
                    }

                    // ウェイトの大きい順で並べ替え
                    IOrderedEnumerable<KeyValuePair<IPXBone, float>> weights = weightsByBone.OrderByDescending(i => i.Value);
                    // 上から4つをBDEFウェイトとして登録
                    foreach(var (p,i)in weights.Indexed())
                    {
                        switch (i)
                        {
                            case 0:
                                pmx.Vertex[selectedVertexIndices[count]].Bone1 = p.Key;
                                pmx.Vertex[selectedVertexIndices[count]].Weight1 = p.Value / nInvDistance.Sum();
                                break;
                            case 1:
                                pmx.Vertex[selectedVertexIndices[count]].Bone2 = p.Key;
                                pmx.Vertex[selectedVertexIndices[count]].Weight2 = p.Value / nInvDistance.Sum();
                                break;
                            case 2:
                                pmx.Vertex[selectedVertexIndices[count]].Bone3 = p.Key;
                                pmx.Vertex[selectedVertexIndices[count]].Weight3 = p.Value / nInvDistance.Sum();
                                break;
                            case 3:
                                pmx.Vertex[selectedVertexIndices[count]].Bone4 = p.Key;
                                pmx.Vertex[selectedVertexIndices[count]].Weight4 = p.Value / nInvDistance.Sum();
                                break;
                            default:
                                break;
                        }
                        if (i >= 3) break;
                    }
                }

                foreach (var item in selectedVertexIndices)
                {
                    Methods.Update(args.Host.Connector, pmx, PmxUpdateObject.Vertex, item);
                }
                MessageBox.Show("完了しました\nウェイトの正規化を行ってください");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private List<IPXFace> GetIncludedFacesOf(IPXVertex v, IPXPmx pmx)
        {
            List<IPXFace> faces = new List<IPXFace>();
            for (int i = 0; i < pmx.Material.Count; i++)
            {
                faces.AddRange(pmx.Material[i].Faces.Where(f => f.Vertex1 == v || f.Vertex2 == v || f.Vertex3 == v).ToList());
            }
            return faces;
        }

        private void setWeight(IPXVertex vertex, int weightID, float invDistance, ref Dictionary<IPXBone, float> dic)
        {
            IPXBone bone;
            float weight;
            switch (weightID)
            {
                case 1:
                    bone = vertex.Bone1;
                    weight = vertex.Weight1;
                    break;
                case 2:
                    bone = vertex.Bone2;
                    weight = vertex.Weight2;
                    break;
                case 3:
                    bone = vertex.Bone3;
                    weight = vertex.Weight3;
                    break;
                case 4:
                    bone = vertex.Bone4;
                    weight = vertex.Weight4;
                    break;
                default:
                    return;
            }

            if (bone == null)
                return;

            if (dic.ContainsKey(bone))
            {
                dic[bone] += weight * invDistance;
            }
            else
            {
                dic[bone] = 0;
            }
        }
    }
}
