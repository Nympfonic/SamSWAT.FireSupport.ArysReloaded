using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.VHACD
{
    [System.Serializable]
    public unsafe struct Parameters
    {
        public void Init()
        {
            m_resolution = 100000;
            m_concavity = 0.001;
            m_planeDownsampling = 4;
            m_convexhullDownsampling = 4;
            m_alpha = 0.05;
            m_beta = 0.05;
            m_pca = 0;
            m_mode = 0; // 0: voxel-based (recommended), 1: tetrahedron-based
            m_maxNumVerticesPerCH = 64;
            m_minVolumePerCH = 0.0001;
            m_callback = null;
            m_logger = null;
            m_convexhullApproximation = 1;
            m_oclAcceleration = 0;
            m_maxConvexHulls = 1024;
            m_projectHullVertices = true; // This will project the output convex hull vertices onto the original source mesh to increase the floating point accuracy of the results
        }

        [Tooltip("Maximum concavity")]
        [Range(0, 1)]
        public double m_concavity;

        [Tooltip("Controls the bias toward clipping along symmetry planes")]
        [Range(0, 1)]
        public double m_alpha;

        [Tooltip("Controls the bias toward clipping along revolution axes")]
        [Range(0, 1)]
        public double m_beta;

        [Tooltip("Controls the adaptive sampling of the generated convex-hulls")]
        [Range(0, 0.01f)]
        public double m_minVolumePerCH;

        public void* m_callback;
        public void* m_logger;

        // This has been reduced in maximum from 64000000 to 6400000
        [Tooltip("Maximum number of voxels generated during the voxelization stage")]
        [Range(10000, 6400000)]
        public uint m_resolution;

        [Tooltip("Controls the maximum number of triangles per convex-hull")]
        [Range(4, 1024)]
        public uint m_maxNumVerticesPerCH;

        [Tooltip("Controls the granularity of the search for the \"best\" clipping plane")]
        [Range(1, 16)]
        public uint m_planeDownsampling;

        [Tooltip("Controls the precision of the convex-hull generation process during the clipping plane selection stage")]
        [Range(1, 16)]
        public uint m_convexhullDownsampling;

        [Tooltip("Enable/disable normalizing the mesh before applying the convex decomposition")]
        [Range(0, 1)]
        public uint m_pca;

        [Tooltip("0: Voxel-based (recommended), 1: Tetrahedron-based")]
        [Range(0, 1)]
        public uint m_mode;

        [Range(0, 1)]
        public uint m_convexhullApproximation;

        [Range(0, 1)]
        public uint m_oclAcceleration;

        [Tooltip("The maximum number of sub-meshes that can be created for this object." +
            " More complex meshes with higher numbers will result in both more computation time and larger file sizes.")]
        [Range(1, 1024)]
        public uint m_maxConvexHulls;

        [Tooltip("This will project the output convex hull vertices onto the original source mesh to increase the floating point accuracy of the results." +
            " This can improve how closely the colliders match your object, but also cause small instances of gaps between colliders.")]
        public bool m_projectHullVertices;
    };
}
