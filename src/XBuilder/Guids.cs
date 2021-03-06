﻿// Guids.cs
// MUST match guids.h
using System;

namespace XBuilder
{
    static class GuidList
    {
        public const string guidModelViewerPkgString = "22f79c21-2040-4b57-af48-7ad329431082";
        public const string guidModelViewerCmdSetString = "bfa7aea4-2696-4bcb-9fbf-31eb90b28c60";
        public const string guidToolWindowPersistanceString = "f358ad4b-049b-4aa3-9646-f13e5e5722f9";

        public static readonly Guid guidModelViewerCmdSet = new Guid(guidModelViewerCmdSetString);

    	//public const string guidContentPreviewToolbarCmdSetString = "B69B565E-A1B2-4716-8B59-7DE9D2299F31";
    	//public static readonly Guid guidContentPreviewToolbarCmdSet = new Guid(guidContentPreviewToolbarCmdSetString);

    	public const string GuidModelEditorLogicalView = "F3A60F97-E9A3-4FDC-B68A-8BF55D356EC1";
		public const string GuidModelEditorFactory = "D85DF2A5-F52E-4F1C-943F-6F9AF4689740";

		public static readonly Guid XBuilderWindowPane = new Guid("3B6E8053-369F-4B90-927C-737299FB382E");
    };
}