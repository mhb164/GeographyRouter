using GeographyModel;

public interface IGeographyRepositoryStorage
{
    bool Save(Layer item);
    bool Delete(Layer item);
    bool DeleteAllLayers();
    bool Save(LayerElement item);

    void WaitFlush();
}
