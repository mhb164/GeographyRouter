using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GeographyModel
{
    public enum LayerElementStatus
    {
        Open = 0,
        Close = 1,
    }

    public partial class LayerElement
    {
        public readonly static List<LayerElement> EmptyList = new List<LayerElement>();

        public readonly Layer Layer;
        public readonly string Code;

        public LayerElement(Layer layer,
                            string code,
                            double[] points,
                            string[] fieldValues,
                            long dataVersion,
                            LayerElementStatus normalStatus,
                            LayerElementStatus actualStatus,
                            long statusVersion)
        {
            Layer = layer;
            Code = code;
            
            Points = points;
            FieldValues = fieldValues;
            DataVersion = dataVersion;

            NormalStatus = normalStatus;
            ActualStatus = actualStatus;
            StatusVersion = statusVersion;
        }

        public void Update(double[] points,
                           string[] fieldValues,
                           LayerElementStatus normalStatus,
                           LayerElementStatus actualStatus,
                           long version)
        {
            Points = points;
            FieldValues = fieldValues;
            NormalStatus = normalStatus;
            ActualStatus = actualStatus;
            DataVersion = StatusVersion = version;
        }

        public void UpdateData(double[] points,
                           string[] fieldValues,
                           long version)
        {
            Points = points;
            FieldValues = fieldValues;
            DataVersion = version;
        }

        public void UpdateStatus(LayerElementStatus normalStatus,
            LayerElementStatus actualStatus,
            long version)
        {
            NormalStatus = normalStatus;
            ActualStatus = actualStatus;
            StatusVersion = version;
        }

        public double[] Points { get; private set; }

        public string[] FieldValues { get; private set; }

        public long DataVersion { get; private set; }

        public LayerElementStatus NormalStatus { get; private set; }

        public LayerElementStatus ActualStatus { get; private set; }

        public long StatusVersion { get; private set; }

        public bool Connected => ActualStatus == LayerElementStatus.Close;

        public override string ToString() => Displayname;
    }
}
