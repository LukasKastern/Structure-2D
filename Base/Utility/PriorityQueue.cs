using System.Collections;
using System.Collections.Generic;
using Structure2D.MapGeneration;

namespace Structure2D.Utility.MapGeneration
{
    /// <summary>
    /// Queue used to store Map Generation Cells based on their priority.
    /// </summary>
    public class PriorityQueue
    {
        private List<QueueCell> list = new List<QueueCell>();
        private HashSet<Coordinate> _usedCoordinates = new HashSet<Coordinate>(); 
        
        private int count = 0;
        private int minimum = int.MaxValue;

        /// <summary>
        /// Amount of Cells currently inside the queue.
        /// </summary>
        public int Count => count;

        public bool IsCellQueueAble(Cell cell) => !_usedCoordinates.Contains(cell.Coordinate);
        
        
        /// <summary>
        /// Enqueues the cell by it's priority.
        /// </summary>
        public void Enqueue (QueueData data)
        {
            _usedCoordinates.Add(data.Cell.Coordinate);
            
            count += 1;
            int priority = data.SearchHeuristic + data.Distance;
            if (priority < minimum) {
                minimum = priority;
            }
            while (priority >= list.Count) {
                list.Add(QueueCell.InvalidCell);
            }

            var queueCell = QueueCell.FromQueueData(data);

            queueCell.NextWithSamePriority = list[priority];
            list[priority] = queueCell;
        }

        /// <summary>
        /// Fetches the Cell with the highest priority.
        /// </summary>
        /// <returns></returns>
        public Cell Dequeue (MapGenerator generator) {
            count -= 1;
            for (; minimum < list.Count; minimum++) {
                QueueCell cell = list[minimum];
                if (cell != null) {
                    list[minimum] = cell.NextWithSamePriority;
                    return cell.ToCell(generator);
                }
            }
            return null;
        }
        
        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear () {
            list.Clear();
            _usedCoordinates.Clear();
            count = 0;
            minimum = int.MaxValue;
        }

        public struct QueueData
        {
            public Cell Cell;
            public int Distance;
            public int SearchHeuristic;
        }

        private class QueueCell
        {
            public static QueueCell InvalidCell = null;
            
            public bool IsValid { get; private set; }

            /// <summary>
            /// Coordinate of this Cell
            /// </summary>
            public Coordinate Coordinate;

            public int SearchHeuristic;

            public QueueCell NextWithSamePriority { get; set; } = null;

            public int Distance { get; set; }

            public Cell ToCell(MapGenerator generator)
            {
                return CellMap.GetCell(Coordinate.x, Coordinate.y);
            }

            public static QueueCell FromMapGenerationCell(Cell cell)
            {
                return new QueueCell() {Coordinate = cell.Coordinate};
            }

            public static QueueCell FromQueueData(QueueData data)
            {
                return new QueueCell()
                {
                    Coordinate = data.Cell.Coordinate,
                    Distance = data.Distance,
                    SearchHeuristic = data.SearchHeuristic,
                };
            }
        }
    }
}