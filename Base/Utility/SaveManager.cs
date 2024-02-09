using System;
using System.IO;
using System.Text;

namespace Structure2D.Utility
{
    /// <summary>
    /// Utility class used to save/load the CellMap from a Stream.
    /// </summary>
    public class SaveManager
    {
        public const double ProjectVersion = 1.0;
        
        /// <summary>
        /// This will be called for every Cell that gets serialized
        /// You can use the given writer to write your own data to the stream.
        /// </summary>
        public static Action<BinaryWriter, Coordinate> OnSavedCell;
    
        /// <summary>
        /// If you use OnSavedCell to save your own custom data use this to read it back when the cells get loaded.
        /// </summary>
        public static Action<BinaryReader, Coordinate> OnLoadedCell;

        /// <summary>
        /// Saves the currently active Map to the given stream
        /// </summary>
        /// <param name="stream">Stream to which we write the Map</param>
        public static void SaveMapToStream(Stream stream)
        {
            if (CellMap.MapWidth == 0 || CellMap.MapHeight == 0)
                return;
            
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                //Write the project version of the map that we serialize
                writer.Write(ProjectVersion);
            
                //Write the dimensions of the map to the stream
                writer.Write(CellMap.MapWidth);
                writer.Write(CellMap.MapHeight);

                for (int x = 0; x < CellMap.MapWidth; ++x)
                {
                    for (int y = 0; y < CellMap.MapHeight; ++y)
                    {
                        SerializeCellTo(CellMap.GetCellUnsafe(x, y), writer);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the CellMap with the Map from the given stream
        /// </summary>
        /// <param name="stream">Stream from which we should load the Map</param>
        /// <param name="allowCrossVersionLoading">Specifies if a Map with a different ProjectVersion can be loaded</param>
        /// <returns></returns>
        public static bool LoadMapFromStream(Stream stream, bool allowCrossVersionLoading = false)
        {
            if (stream.Length == 0)
                return false;
        
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, true))
            {
                var mapVersion = reader.ReadDouble();

                if (!allowCrossVersionLoading && mapVersion != ProjectVersion)
                    return false;
            
                int mapWidth = reader.ReadInt32();
                int mapHeight = reader.ReadInt32();

                //Can't load an empty map
                if (mapHeight <= 0 || mapWidth <= 0)
                    return false;
            

                CellMap.CreateMap(mapWidth / CellMetrics.ChunkSize, mapHeight / CellMetrics.ChunkSize);
                
                for (int x = 0; x < mapWidth; ++x)
                {
                    for (int y = 0; y < mapHeight; ++y)
                    {
                        DeserializeCellFrom(CellMap.GetCellUnsafe(x, y), reader);
                    }
                }
                
                CellMap.SetMapVisible();
            }

            return true;
        }

        public static bool CanLoadMap(Stream stream, out MapData mapData, bool allowCrossVersionLoading = false)
        {
            mapData = new MapData();
            
            if (stream.Length == 0)
                return false;

            var streamPosition = stream.Position;

            try
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.Default, true))
                {
                    mapData.MapVersion = reader.ReadDouble();

                    if (!allowCrossVersionLoading && mapData.MapVersion != ProjectVersion)
                        return false;
            
                    mapData.MapWidth  = reader.ReadInt32();
                    mapData.MapHeight = reader.ReadInt32();

                
                
                    //Can't load an empty map
                    if (mapData.MapWidth <= 0 || mapData.MapWidth <= 0)
                        return false;
                }
            }
            
            catch (Exception)
            {
                return false;
            }

            stream.Position = streamPosition;
            
            return true;
        }
        
        /// <summary>
        /// Writes the cell data into the given writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="cell"></param>
        private static void SerializeCellTo(Cell cell, BinaryWriter writer)
        {
            writer.Write(cell.Block);
            writer.Write(cell.Background);
        
            OnSavedCell?.Invoke(writer, cell.Coordinate);
        }

        /// <summary>
        /// Initializes this data from the given reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cell"></param>
        private static void DeserializeCellFrom(Cell cell, BinaryReader reader)
        {
            cell.Block = reader.ReadInt32();
            cell.Background = reader.ReadInt32();
        
            OnLoadedCell?.Invoke(reader, cell.Coordinate);
        }

    }

    public struct MapData
    {
        public int MapWidth;
        public int MapHeight;
        public double MapVersion;
    }
}