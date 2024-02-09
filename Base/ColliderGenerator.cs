using System.Collections.Generic;
using Structure2D;
using Structure2D.Utility;
using UnityEngine;

namespace Structure2D
{
    internal class ColliderGenerator
    {
        private static Vector2[] points = new Vector2[5];

        /// <summary>
        /// Generates colliders for the given chunk and adds them to the given list.
        /// The list gets cleared before the new colliders are added
        /// </summary>
        /// <param name="chunkToBuildCollidersFor"></param>
        /// <param name="collider2Ds"></param>
        internal static void GenerateColliders(Chunk chunkToBuildCollidersFor, List<EdgeCollider2D> collider2Ds)
        {
            chunkToBuildCollidersFor.ClearColliders();
            var bounds = FindColliderBounds(chunkToBuildCollidersFor);

            foreach (var bound in bounds)
            {
                if (!bound.IsValid)
                    continue;

                var topLeftXPosition = bound.Start.x * CellMetrics.CellSize;
                var topLeftYPosition = bound.Start.y * CellMetrics.CellSize;

                var topRightXPosition = topLeftXPosition + bound.Width * CellMetrics.CellSize;
                var topRightYPosition = topLeftYPosition;

                var bottomLeftXPosition = topLeftXPosition;
                var bottomLeftYPosition = topLeftYPosition + bound.CellsDown * CellMetrics.CellSize;

                var bottomRightXPosition = topRightXPosition;
                var bottomRightYPosition = bottomLeftYPosition;

                var edgeCollider = ColliderPool.Get();

                //We use a static vector2 of points so we don't allocate more than we have to
                points[0] = new Vector2(topLeftXPosition, topLeftYPosition);
                points[1] = new Vector2(topRightXPosition, topRightYPosition);
                points[2] = new Vector2(bottomRightXPosition, bottomRightYPosition);
                points[3] = new Vector2(bottomLeftXPosition, bottomLeftYPosition);
                points[4] = new Vector2(topLeftXPosition, topLeftYPosition);


                edgeCollider.points = points;

                edgeCollider.offset = new Vector2(chunkToBuildCollidersFor.Offset.x, chunkToBuildCollidersFor.Offset.y) *
                                      (CellMetrics.CellSize * CellMetrics.ChunkSize);

                collider2Ds.Add(edgeCollider);
            }

            ListPool<ColliderBounds>.Add(bounds);
        }

        /// <summary>
        /// This function searches for all solid cells inside the given array and creates colliders from them.
        /// These colliders are already merged with the MergeColliders function
        /// </summary>
        /// <param name="Cells"></param>
        /// <returns></returns>
        private static List<ColliderBounds> FindColliderBounds(Chunk chunk)
        {
            var colliders = ListPool<ColliderBounds>.Get();

            var currentBound = new ColliderBounds();

            for (int x = 0; x < CellMetrics.ChunkSize; ++x)
            {
                for (int y = 0; y < CellMetrics.ChunkSize; ++y)
                {
                    //Break the current collider
                    if (!BlockSolidStateMetaData.IsBlockSolid[CellMap.GetCell(x + chunk.CellOffset.x, y + chunk.CellOffset.y).Block])
                    {
                        if (currentBound.IsValid)
                            colliders.Add(currentBound);

                        currentBound.IsValid = false;
                    }

                    else
                    {
                        //Create a new collider if there is no active one
                        if (!currentBound.IsValid)
                        {
                            currentBound = new ColliderBounds
                            {
                                Start = new Vector2Int(x, y),
                                IsValid = true,
                                Width = 1,
                            };
                        }

                        ++currentBound.CellsDown;

                    }
                }

                if (currentBound.IsValid)
                {
                    colliders.Add(currentBound);
                    currentBound.IsValid = false;
                }
            }

            return MergeColliders(colliders);
        }

        /// <summary>
        /// Merges colliders of the given list with the same height and length together
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private static List<ColliderBounds> MergeColliders(List<ColliderBounds> bounds)
        {
            //We don't remove the colliders from the list but rather mark them as not valid
            //This way we don't have to change anything inside the loop

            if (bounds.Count <= 1)
                return bounds;

            List<ColliderBounds> possibleMerges = ListPool<ColliderBounds>.Get();
            List<int> possibleMergesIndices = ListPool<int>.Get();


            for (int i = 0; i < bounds.Count; ++i)
            {
                //If the bounds is already used we don't have to check it
                if (!bounds[i].IsValid)
                    continue;

                var bound = bounds[i];

                for (int j = i + 1; j < bounds.Count; ++j)
                {
                    //Bound is already merged with a different bound or it doesnt start at the same index as this one
                    if (!bounds[j].IsValid || bound.Start.y != bounds[j].Start.y || bounds[j].CellsDown != bound.CellsDown)
                        continue;

                    //We still have to check if the x of the bounds will be next to use,
                    //so we add it to the possible merges to check this in the next step
                    possibleMerges.Add(bounds[j]);
                    possibleMergesIndices.Add(j);
                }

                //Now we sort our merges by their x
                possibleMerges.Sort((a, b) => a.Start.x.CompareTo(b.Start.x));

                for (int j = 0; j < possibleMerges.Count; ++j)
                {
                    //Collider is not next to us so we can't merge with it and therefor all other colliders are also not next to use
                    if (possibleMerges[j].Start.x - bound.Width != bound.Start.x)
                    {
                        break;
                    }

                    //Otherwise we can increase our width and mark the merged collider as used
                    bounds[possibleMergesIndices[j]] = new ColliderBounds();

                    ++bound.Width;
                }

                //Fill our new bounds back into the array
                bounds[i] = bound;

                possibleMerges.Clear();
                possibleMergesIndices.Clear();
            }

            ListPool<ColliderBounds>.Add(possibleMerges);
            ListPool<int>.Add(possibleMergesIndices);

            return bounds;
        }

        private struct ColliderBounds
        {
            public Vector2Int Start;
            public int CellsDown;
            public int Width;
            public bool IsValid;
        }
    }
}