// Guids.cs
// MUST match guids.h
using System;

namespace FormosatekLtd.ModelViewer
{
    static class GuidList
    {
        public const string guidModelViewerPkgString = "22f79c21-2040-4b57-af48-7ad329431082";
        public const string guidModelViewerCmdSetString = "bfa7aea4-2696-4bcb-9fbf-31eb90b28c60";
        public const string guidToolWindowPersistanceString = "f358ad4b-049b-4aa3-9646-f13e5e5722f9";

        public static readonly Guid guidModelViewerCmdSet = new Guid(guidModelViewerCmdSetString);
    };
}