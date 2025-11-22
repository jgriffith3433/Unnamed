using System.Runtime.CompilerServices;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public struct JobCameraData
    {
        public const ulong Size = (sizeof(float) * 34);
        public float3 CamPos;
        public float3 LightDir;
        public float CellSize;

        //Cam Planes
        public float3 fp0;
        public float fp0Dist;
        public float3 fp1;
        public float fp1Dist;
        public float3 fp2;
        public float fp2Dist;
        public float3 fp3;
        public float fp3Dist;
        public float3 fp4;
        public float fp4Dist;
        public float3 fp5;
        public float fp5Dist;
        
        public static void ExpandBounds(float3 extents, float3 Dir, float distance, out float3 outCenter, out float3 outExtents)
        {
            float3 max = extents;
            float3 min = -extents;

            // Only expand in the direction we're turning
            max += math.max(float3.zero, Dir) * distance;
            min += math.min(float3.zero, Dir) * distance;

            outExtents = (max - min) * 0.5f;
            outCenter = (min + outExtents);
        }

        /// <summary>
        /// Tests if an Axis-Aligned Bounding Box (AABB) is visible within the camera's view frustum.
        /// This method performs both vertex and edge intersection tests for accurate visibility determination.
        /// </summary>
        /// <param name="center">The center point of the AABB in world space</param>
        /// <param name="extents">Half-sizes of the AABB in each dimension (x,y,z)</param>
        /// <param name="distance">Distance to the AABB from the camera to modify the camera turn scaling</param>
        /// <param name="allVisible">Set to true if the entire AABB is inside all frustum planes, false if any part is outside</param>
        /// <returns>True if any part of the AABB (vertices or edges) is visible, false if completely outside the frustum</returns>
        public bool IsAABBVisible(float3 center, float3 extents, float distance, out bool allVisible)
        {
            allVisible = true;

            // Pre-calculate all 8 corners of the AABB using local variables for better vectorization
            float3 c000 = center + new float3(-extents.x, -extents.y, -extents.z);
            float3 c100 = center + new float3(extents.x, -extents.y, -extents.z);
            float3 c010 = center + new float3(-extents.x, extents.y, -extents.z);
            float3 c110 = center + new float3(extents.x, extents.y, -extents.z);
            float3 c001 = center + new float3(-extents.x, -extents.y, extents.z);
            float3 c101 = center + new float3(extents.x, -extents.y, extents.z);
            float3 c011 = center + new float3(-extents.x, extents.y, extents.z);
            float3 c111 = center + new float3(extents.x, extents.y, extents.z);

            // Test against each frustum plane (Left, Right, Bottom, Top, Near, Far)
            if (!TestAgainstPlane(fp0, fp0Dist, ref allVisible, 
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp1, fp1Dist, ref allVisible,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp2, fp2Dist, ref allVisible,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp3, fp3Dist, ref allVisible,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp4, fp4Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp5, fp5Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;

            return true;
        }
        /// <summary>
        /// Tests if an Axis-Aligned Bounding Box (AABB) is visible within the camera's view frustum.
        /// This method performs both vertex and edge intersection tests for accurate visibility determination.
        /// </summary>
        /// <param name="center">The center point of the AABB in world space</param>
        /// <param name="extents">Half-sizes of the AABB in each dimension (x,y,z)</param>
        /// <param name="distance">Distance to the AABB from the camera to modify the camera turn scaling</param>
        /// <returns>True if any part of the AABB (vertices or edges) is visible, false if completely outside the frustum</returns>
        public bool IsAABBVisible(float3 center, float3 extents, float distance)
        {
            // Pre-calculate all 8 corners of the AABB using local variables for better vectorization
            float3 c000 = center + new float3(-extents.x, -extents.y, -extents.z);
            float3 c100 = center + new float3(extents.x, -extents.y, -extents.z);
            float3 c010 = center + new float3(-extents.x, extents.y, -extents.z);
            float3 c110 = center + new float3(extents.x, extents.y, -extents.z);
            float3 c001 = center + new float3(-extents.x, -extents.y, extents.z);
            float3 c101 = center + new float3(extents.x, -extents.y, extents.z);
            float3 c011 = center + new float3(-extents.x, extents.y, extents.z);
            float3 c111 = center + new float3(extents.x, extents.y, extents.z);

            // Test against each frustum plane
            if (!TestAgainstPlane(fp0, fp0Dist, 
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp1, fp1Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp2, fp2Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp3, fp3Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp4, fp4Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;
                
            if (!TestAgainstPlane(fp5, fp5Dist,
                c000, c100, c010, c110, c001, c101, c011, c111)) return false;

            return true;
        }

        /// <summary>
        /// Tests an AABB against a single frustum plane, checking both vertices and edges for intersection.
        /// </summary>
        /// <param name="planeNormal">Normal vector of the frustum plane</param>
        /// <param name="planeDist">Distance of the frustum plane from origin</param>
        /// <param name="allVisible">Reference to the all-visible flag, updated based on test results</param>
        /// <param name="c000">Corner (-X, -Y, -Z)</param>
        /// <param name="c100">Corner (+X, -Y, -Z)</param>
        /// <param name="c010">Corner (-X, +Y, -Z)</param>
        /// <param name="c110">Corner (+X, +Y, -Z)</param>
        /// <param name="c001">Corner (-X, -Y, +Z)</param>
        /// <param name="c101">Corner (+X, -Y, +Z)</param>
        /// <param name="c011">Corner (-X, +Y, +Z)</param>
        /// <param name="c111">Corner (+X, +Y, +Z)</param>
        /// <returns>True if any part of the AABB is visible against this plane, false if completely outside</returns>
        private bool TestAgainstPlane(float3 planeNormal, float planeDist, 
            ref bool allVisible,
            float3 c000, float3 c100, float3 c010, float3 c110,
            float3 c001, float3 c101, float3 c011, float3 c111)
        {

            // Test all corners against this plane - unrolled for better performance
            float d000 = math.dot(planeNormal, c000) + planeDist;
            float d100 = math.dot(planeNormal, c100) + planeDist;
            float d010 = math.dot(planeNormal, c010) + planeDist;
            float d110 = math.dot(planeNormal, c110) + planeDist;
            float d001 = math.dot(planeNormal, c001) + planeDist;
            float d101 = math.dot(planeNormal, c101) + planeDist;
            float d011 = math.dot(planeNormal, c011) + planeDist;
            float d111 = math.dot(planeNormal, c111) + planeDist;

            // Count corners inside the plane - unrolled for vectorization
            int cornersInside = 0;
            cornersInside += math.select(0, 1, d000 >= 0);
            cornersInside += math.select(0, 1, d100 >= 0);
            cornersInside += math.select(0, 1, d010 >= 0);
            cornersInside += math.select(0, 1, d110 >= 0);
            cornersInside += math.select(0, 1, d001 >= 0);
            cornersInside += math.select(0, 1, d101 >= 0);
            cornersInside += math.select(0, 1, d011 >= 0);
            cornersInside += math.select(0, 1, d111 >= 0);

            if(allVisible && cornersInside < 8)
                allVisible = false;

            // If no corners are inside, check edges for intersection
            if (cornersInside == 0)
            {
                // Check all 12 edges for intersection - unrolled for better performance
                // Edges along X axis
                if (CheckEdgeIntersection(d000, d100)) return true;
                if (CheckEdgeIntersection(d010, d110)) return true;
                if (CheckEdgeIntersection(d001, d101)) return true;
                if (CheckEdgeIntersection(d011, d111)) return true;

                // Edges along Y axis
                if (CheckEdgeIntersection(d000, d010)) return true;
                if (CheckEdgeIntersection(d100, d110)) return true;
                if (CheckEdgeIntersection(d001, d011)) return true;
                if (CheckEdgeIntersection(d101, d111)) return true;

                // Edges along Z axis
                if (CheckEdgeIntersection(d000, d001)) return true;
                if (CheckEdgeIntersection(d100, d101)) return true;
                if (CheckEdgeIntersection(d010, d011)) return true;
                if (CheckEdgeIntersection(d110, d111)) return true;

                return false;
            }

            return true;
        }
        /// <summary>
        /// Tests an AABB against a single frustum plane, checking both vertices and edges for intersection.
        /// </summary>
        /// <param name="planeNormal">Normal vector of the frustum plane</param>
        /// <param name="planeDist">Distance of the frustum plane from origin</param>
        /// <param name="c000">Corner (-X, -Y, -Z)</param>
        /// <param name="c100">Corner (+X, -Y, -Z)</param>
        /// <param name="c010">Corner (-X, +Y, -Z)</param>
        /// <param name="c110">Corner (+X, +Y, -Z)</param>
        /// <param name="c001">Corner (-X, -Y, +Z)</param>
        /// <param name="c101">Corner (+X, -Y, +Z)</param>
        /// <param name="c011">Corner (-X, +Y, +Z)</param>
        /// <param name="c111">Corner (+X, +Y, +Z)</param>
        /// <returns>True if any part of the AABB is visible against this plane, false if completely outside</returns>
        private bool TestAgainstPlane(float3 planeNormal, float planeDist,
            float3 c000, float3 c100, float3 c010, float3 c110,
            float3 c001, float3 c101, float3 c011, float3 c111)
        {

            // Test all corners against this plane - unrolled for better performance
            float d000 = math.dot(planeNormal, c000) + planeDist;
            float d100 = math.dot(planeNormal, c100) + planeDist;
            float d010 = math.dot(planeNormal, c010) + planeDist;
            float d110 = math.dot(planeNormal, c110) + planeDist;
            float d001 = math.dot(planeNormal, c001) + planeDist;
            float d101 = math.dot(planeNormal, c101) + planeDist;
            float d011 = math.dot(planeNormal, c011) + planeDist;
            float d111 = math.dot(planeNormal, c111) + planeDist;

            // Count corners inside the plane - unrolled for vectorization
            int cornersInside = 0;
            cornersInside += math.select(0, 1, d000 >= 0);
            cornersInside += math.select(0, 1, d100 >= 0);
            cornersInside += math.select(0, 1, d010 >= 0);
            cornersInside += math.select(0, 1, d110 >= 0);
            cornersInside += math.select(0, 1, d001 >= 0);
            cornersInside += math.select(0, 1, d101 >= 0);
            cornersInside += math.select(0, 1, d011 >= 0);
            cornersInside += math.select(0, 1, d111 >= 0);

            // If no corners are inside, check edges for intersection
            if (cornersInside == 0)
            {
                // Check all 12 edges for intersection - unrolled for better performance
                // Edges along X axis
                if (CheckEdgeIntersection(d000, d100)) return true;
                if (CheckEdgeIntersection(d010, d110)) return true;
                if (CheckEdgeIntersection(d001, d101)) return true;
                if (CheckEdgeIntersection(d011, d111)) return true;

                // Edges along Y axis
                if (CheckEdgeIntersection(d000, d010)) return true;
                if (CheckEdgeIntersection(d100, d110)) return true;
                if (CheckEdgeIntersection(d001, d011)) return true;
                if (CheckEdgeIntersection(d101, d111)) return true;

                // Edges along Z axis
                if (CheckEdgeIntersection(d000, d001)) return true;
                if (CheckEdgeIntersection(d100, d101)) return true;
                if (CheckEdgeIntersection(d010, d011)) return true;
                if (CheckEdgeIntersection(d110, d111)) return true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Tests if an edge intersects a plane by checking if its endpoints are on opposite sides of the plane.
        /// A point exactly on the plane (distance = 0) is considered visible.
        /// </summary>
        /// <param name="d1">Distance of first endpoint from plane</param>
        /// <param name="d2">Distance of second endpoint from plane</param>
        /// <returns>True if the edge intersects the plane or if either endpoint lies on the plane</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckEdgeIntersection(float d1, float d2)
        {
            // Consider points on the plane (distance = 0) as visible
            return d1 >= 0 || d2 >= 0 || (math.sign(d1) != math.sign(d2));
        }

        /// <summary>
        /// Tests if a sphere is visible within the camera's view frustum.
        /// This method performs an exact sphere-plane test against all frustum planes.
        /// The plane normals should point inward (into the frustum).
        /// </summary>
        /// <param name="center">The center point of the sphere in world space</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="fullyVisible">Set to true if the entire sphere is inside all frustum planes</param>
        /// <returns>True if any part of the sphere is visible, false if completely outside the frustum</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSphereVisible(float3 center, float radius, out bool fullyVisible)
        {
            fullyVisible = false;
            
            // Calculate signed distances from sphere center to each plane
            // Note: For a frustum, plane normals point inward, so:
            // - Negative distance means outside the plane
            // - Positive distance means inside the plane
            float d0 = math.dot(fp0, center) + fp0Dist;
            float d1 = math.dot(fp1, center) + fp1Dist;
            float d2 = math.dot(fp2, center) + fp2Dist;
            float d3 = math.dot(fp3, center) + fp3Dist;
            float d4 = math.dot(fp4, center) + fp4Dist;
            float d5 = math.dot(fp5, center) + fp5Dist;

            // Early out: if sphere is completely outside any plane, it's not visible
            if (d0 < -radius ||
                d1 < -radius ||
                d2 < -radius ||
                d3 < -radius ||
                d4 < -radius ||
                d5 < -radius)
                return false;

            // If we got here, the sphere is at least partially visible
            // Check if it's fully inside all planes (distance > radius for all planes)
            fullyVisible = (d0 > radius) && 
                          (d1 > radius) && 
                          (d2 > radius) && 
                          (d3 > radius) && 
                          (d4 > radius) && 
                          (d5 > radius);

            return true;
        }
        /// <summary>
        /// Tests if a sphere is visible within the camera's view frustum.
        /// This method performs an exact sphere-plane test against all frustum planes.
        /// The plane normals should point inward (into the frustum).
        /// </summary>
        /// <param name="center">The center point of the sphere in world space</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="distance">Distance to the sphere from the camera to modify the camera turn scaling</param>
        /// <returns>True if any part of the sphere is visible, false if completely outside the frustum</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSphereVisible(float3 center, float radius, float distance)
        {
            if (math.dot(fp0, center) + fp0Dist < -radius)
                return false;
            if (math.dot(fp1, center) + fp1Dist < -radius)
                return false;
            if (math.dot(fp2, center) + fp2Dist < -radius)
                return false;
            if (math.dot(fp3, center) + fp3Dist < -radius)
                return false;
            if (math.dot(fp4, center) + fp4Dist < -radius)
                return false;
            if (math.dot(fp5, center) + fp5Dist < -radius)
                return false;

            return true;
        }
    }
}