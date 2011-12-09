using System;

namespace Cloudy.Computing.Topologies.Shortcuts
{
    /// <summary>
    /// Shortcuts for the Star topology.
    /// </summary>
    public static class StarShortcuts
    {
        /// <summary>
        /// The central node.
        /// </summary>
        public static readonly Guid Center = 
            new Guid("{8F696BC7-761E-426B-B73C-F4D02359C0B5}");

        /// <summary>
        /// Nodes connected to the central node.
        /// </summary>
        public static readonly Guid Peripherals =
            new Guid("{1BE88CAF-72EC-40D7-B2DE-30953041AD94}");
    }
}
