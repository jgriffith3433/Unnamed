ProceduralWorlds Storm Geospatial Library
==========================================

A comprehensive Unity geospatial library for accurate coordinate conversions, calculations, and data package management.

Library Structure
=================

Core Data Types (GeoTypes.cs)
-----------------------------
- GeoLatLng - Geographic coordinate pair with double precision
- GeoBounds - Geographic bounds with utility methods
- GeoCentroid - Center point coordinates
- GeoBoundingBox - Explicit bounding box with conversion methods
- GeoSelectionSize - Region dimensions in meters
- GeoDimensions - Data file dimensions

Constants (GeoConstants.cs)
---------------------------
- Earth Parameters - WGS84 ellipsoid constants
- Conversion Factors - Degrees/radians, meters per degree
- Utility Methods - Earth radius calculations, longitude scaling

Calculations (GeoCalculations.cs)
---------------------------------
Static methods for geospatial calculations:
- Distance - Vincenty's formulae for millimeter-accurate calculations
- Area - Polygon and bounds area calculations
- Bearing & Offset - Navigation calculations
- Bounds - Geographic bounds creation and measurement
- Utilities - Interpolation, formatting, coordinate shifting

Coordinate Conversion (GeoUtils.cs)
-----------------------------------
Main utility class for Unity-geographic coordinate conversion:
- Constructor - Initialize with geographic origin
- Coordinate Conversion - Unity ↔ Geographic coordinates
- Bounds Conversion - Bounding box to Unity bounds
- Distance Calculations - Unity world position distances
- Area Calculations - Unity polygon areas
- Floating Origin - Support for massive worlds

Data Package Support
====================

Main Package (GeoPackage.cs)
-----------------------------
- GeoPackage - Main data package container

Metadata (GeoPackageMetadata.cs)
---------------------------------
- GeoPackageMetadata - Package metadata
- GeographicMetadata - File-specific geographic metadata
- ProvenanceInfo - Data provenance and quality information
- QualityScore - Provider quality metrics

Distribution (GeoDistribution.cs)
---------------------------------
- GeoDistribution - File distribution information with utility methods

Usage Examples
==============

Basic Coordinate Conversion
----------------------------
// Initialize with geographic origin
GeoUtils geo = new GeoUtils(37.7749, -122.4194); // San Francisco

// Convert Unity position to lat/lng
Vector3 unityPos = new Vector3(1000, 0, -500);
GeoLatLng coords = geo.UnityToLatLng(unityPos);

// Convert lat/lng to Unity position
Vector3 worldPos = geo.LatLngToUnity(37.7849, -122.4094);

Distance Calculations
---------------------
// Calculate distance between two geographic points
GeoLatLng point1 = new GeoLatLng(37.7749, -122.4194);
GeoLatLng point2 = new GeoLatLng(37.7849, -122.4094);
double distance = GeoCalculations.CalculateDistance(point1, point2);

// Calculate distance between Unity positions
double unityDistance = geo.CalculateDistanceUnity(pos1, pos2);

Area Calculations
-----------------
// Calculate polygon area from geographic coordinates
GeoLatLng[] polygon = { /* ... */ };
double area = GeoCalculations.CalculatePolygonArea(polygon);

// Calculate area from Unity positions
Vector3[] unityPolygon = { /* ... */ };
double unityArea = geo.CalculatePolygonAreaUnity(unityPolygon);

Bounds Operations
-----------------
// Create bounds from center and dimensions
GeoLatLng center = new GeoLatLng(37.7749, -122.4194);
GeoBounds bounds = GeoCalculations.CreateBoundsFromCenter(center, 10000, 8000);

// Check if point is within bounds
bool isInside = bounds.Contains(testPoint);

// Convert to Unity bounds
Bounds unityBounds = geo.BBoxToUnityBounds(bounds.ToArray());

Working with GeoPackages
-------------------------
// Load GeoPackage from JSON
string jsonData = File.ReadAllText("data-summary.json");
GeoPackage package = JsonUtility.FromJson<GeoPackage>(jsonData);

// Initialize GeoUtils with package centroid
GeoUtils geo = new GeoUtils(
    package.metadata.centroid.latitude,
    package.metadata.centroid.longitude
);

// Process distributions
foreach (var distribution in package.distributions)
{
    if (distribution.IsSpatialData())
    {
        Debug.Log($"Spatial file: {distribution.name} ({distribution.GetFormattedSize()})");
    }
}

Floating Origin Support
------------------------
// Set floating origin for massive worlds
geo.SetFloatingOrigin(new Vector3(10000, 0, 5000));

// All coordinate conversions automatically account for floating origin
GeoLatLng coords = geo.UnityToLatLng(unityPosition); // Handles offset internally

Key Features
============

- High Precision - Double precision calculations throughout
- WGS84 Compliant - Uses accurate Earth ellipsoid model
- Unity Integration - Seamless Unity world space conversion
- Floating Origin - Support for massive worlds
- Comprehensive - Distance, area, bearing, and bounds calculations
- Data Package Support - Full GeoPackage metadata handling
- Well Organized - Clean separation of concerns across multiple files
- Extensible - Easy to add new functionality
- Conflict-Free - All types prefixed with "Geo" to avoid Unity naming conflicts

Coordinate System
=================

- Unity Units - 1 Unity unit = 1 meter
- Geographic Origin - Maps to Unity origin (0, 0, 0)
- Axes - X = East/West (longitude), Z = North/South (latitude), Y = Elevation
- Projection - Direct meter-based conversion from geographic origin 