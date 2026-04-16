using System;

namespace ObrazkyJakCyp
{
    internal static class Extensions
    {
        public static bool IsPainting(this GrabbableObject obj) => obj.itemProperties.itemName == "Painting";
    }
}
