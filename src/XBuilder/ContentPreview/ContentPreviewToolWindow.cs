using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using XBuilder.Vsx;
using XBuilder.Xna;

namespace XBuilder.ContentPreview
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("f358ad4b-049b-4aa3-9646-f13e5e5722f9")]
    public class ContentPreviewToolWindow : ToolWindowPane
    {
    	private OleMenuCommand _normals;
		private OleMenuCommand _boundingBox;
		private OleMenuCommand _alphaBlend;
        private OleMenuCommand _textureSize;

		private ShadingMode _shadingMode;
		private Primitive _primitive;
    	private OleMenuCommand _fillModeSolid;
    	private OleMenuCommand _fillModeWireframe;
    	private OleMenuCommand _fillModeSolidAndWireframe;
    	private OleMenuCommand _primitiveSphere;
    	private OleMenuCommand _primitiveCube;
    	private OleMenuCommand _primitiveCylinder;
    	private OleMenuCommand _primitiveTorus;
    	private OleMenuCommand _primitivePlane;
    	private OleMenuCommand _primitiveTeapot;

    	/// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ContentPreviewToolWindow() :
            base(null)
        {
        	ToolBar = new CommandID(
        		GuidList.guidModelViewerCmdSet,
        		PkgCmdIDList.cmdidContentPreviewToolbar);

            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 303;
            this.BitmapIndex = 0;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
			base.Content = new ContentPreviewToolWindowControl();
        }

		public override void OnToolWindowCreated()
		{
			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = ((XBuilderPackage) Package).GetService<IMenuCommandService>() as OleMenuCommandService;
			if (mcs != null)
			{
				_fillModeSolid = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarFillModeSolid, ChangeFillModeSolid);
				_fillModeWireframe = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarFillModeWireframe, ChangeFillModeWireframe);
				_fillModeSolidAndWireframe = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarFillModeSolidAndWireframe, ChangeFillModeSolidAndWireframe);

				_normals = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarNormals, ToggleNormals);
				_boundingBox = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarBoundingBox, ToggleBoundingBox);
				_alphaBlend = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarAlphaBlend, ToggleAlphaBlend);
                _textureSize = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarTextureSize, ToggleTextureSize);

				_primitiveSphere = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarPrimitiveSphere, ChangePrimitiveSphere);
				_primitiveCube = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarPrimitiveCube, ChangePrimitiveCube);
				_primitiveCylinder = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarPrimitiveCylinder, ChangePrimitiveCylinder);
				_primitiveTorus = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarPrimitiveTorus, ChangePrimitiveTorus);
				_primitivePlane = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarPrimitivePlane, ChangePrimitivePlane);
				_primitiveTeapot = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarPrimitiveTeapot, ChangePrimitiveTeapot);

				_fillModeSolid.Enabled = false;
				_fillModeWireframe.Enabled = false;
				_fillModeSolidAndWireframe.Enabled = false;
				_normals.Enabled = false;
				_alphaBlend.Enabled = false;
                _textureSize.Enabled = false;
				_boundingBox.Enabled = false;
				_primitiveSphere.Enabled = false;
				_primitiveCube.Enabled = false;
				_primitiveCylinder.Enabled = false;
				_primitiveTorus.Enabled = false;
				_primitivePlane.Enabled = false;
				_primitiveTeapot.Enabled = false;
			}

			((ContentPreviewToolWindowControl)base.Content).Initialize((XBuilderPackage) Package);

			base.OnToolWindowCreated();
		}

		private static OleMenuCommand AddCommand(IMenuCommandService menuCommandService, int commandID, EventHandler eventHandler)
		{
			CommandID menuCommandID = new CommandID(GuidList.guidModelViewerCmdSet, commandID);
			OleMenuCommand menuItem = new OleMenuCommand(eventHandler, menuCommandID);
			menuCommandService.AddCommand(menuItem);
			return menuItem;
		}

		private void ChangeFillModeSolid(object sender, EventArgs e)
		{
			_shadingMode = ShadingMode.Solid;
			ChangeFillMode();
		}

		private void ChangeFillModeWireframe(object sender, EventArgs e)
		{
			_shadingMode = ShadingMode.Wireframe;
			ChangeFillMode();
		}

		private void ChangeFillModeSolidAndWireframe(object sender, EventArgs e)
		{
			_shadingMode = ShadingMode.SolidAndWireframe;
			ChangeFillMode();
		}

		private void ChangePrimitiveSphere(object sender, EventArgs e)
		{
			_primitive = Primitive.Sphere;
			ChangePrimitive();
		}

		private void ChangePrimitiveCube(object sender, EventArgs e)
		{
			_primitive = Primitive.Cube;
			ChangePrimitive();
		}

		private void ChangePrimitiveCylinder(object sender, EventArgs e)
		{
			_primitive = Primitive.Cylinder;
			ChangePrimitive();
		}

		private void ChangePrimitiveTorus(object sender, EventArgs e)
		{
			_primitive = Primitive.Torus;
			ChangePrimitive();
		}

		private void ChangePrimitivePlane(object sender, EventArgs e)
		{
			_primitive = Primitive.Plane;
			ChangePrimitive();
		}

		private void ChangePrimitiveTeapot(object sender, EventArgs e)
		{
			_primitive = Primitive.Teapot;
			ChangePrimitive();
		}

		private void ToggleNormals(object sender, EventArgs e)
		{
			_normals.Checked = !_normals.Checked;
			ShowNormals();
		}

		private void ToggleBoundingBox(object sender, EventArgs e)
		{
			_boundingBox.Checked = !_boundingBox.Checked;
			ShowBoundingBox();
		}

		private void ToggleAlphaBlend(object sender, EventArgs e)
		{
			_alphaBlend.Checked = !_alphaBlend.Checked;
			ToggleAlphaBlend();
		}

        private void ToggleTextureSize(object sender, EventArgs e)
        {
            _textureSize.Checked = !_textureSize.Checked;
            ToggleTextureSize();
        }

		public void LoadFile(string fileName, XnaBuildProperties buildProperties)
		{
			_fillModeSolid.Enabled = true;
			_fillModeWireframe.Enabled = true;
			_fillModeSolidAndWireframe.Enabled = true;
			_normals.Enabled = true;
			_alphaBlend.Enabled = true;
            _textureSize.Enabled = true;
			_boundingBox.Enabled = true;
			_primitiveSphere.Enabled = true;
			_primitiveCube.Enabled = true;
			_primitiveCylinder.Enabled = true;
			_primitiveTorus.Enabled = true;
			_primitivePlane.Enabled = true;
			_primitiveTeapot.Enabled = true;

			AssetType assetType = ((ContentPreviewToolWindowControl)base.Content).LoadFile(fileName, buildProperties);

			switch (assetType)
			{
				case AssetType.Model:
					_primitiveSphere.Enabled = false;
					_primitiveCube.Enabled = false;
					_primitiveCylinder.Enabled = false;
					_primitiveTorus.Enabled = false;
					_primitivePlane.Enabled = false;
					_primitiveTeapot.Enabled = false;

                    _normals.Visible = true;
                    _textureSize.Enabled = false;
                    _textureSize.Visible = false;
					break;
				case AssetType.Texture:
					_fillModeSolid.Enabled = false;
					_fillModeWireframe.Enabled = false;
					_fillModeSolidAndWireframe.Enabled = false;
					_normals.Enabled = false;
                    _normals.Visible = false;
					_alphaBlend.Enabled = false;
					_boundingBox.Enabled = false;
					_primitiveSphere.Enabled = false;
					_primitiveCube.Enabled = false;
					_primitiveCylinder.Enabled = false;
					_primitiveTorus.Enabled = false;
					_primitivePlane.Enabled = false;
					_primitiveTeapot.Enabled = false;


                    _textureSize.Enabled = true;
                    _textureSize.Visible = true;
					break;
			}

			//bool isModelLoaded = ((ContentPreviewToolWindowControl) base.Content).IsModelLoaded;
			//_fillModeSolid.Enabled = _fillModeWireframe.Enabled = isModelLoaded;

			ChangeFillMode();
			ChangePrimitive();
			ShowNormals();
			ShowBoundingBox();
		}

    	private void ChangeFillMode()
    	{
			((ContentPreviewToolWindowControl)base.Content).ChangeFillMode(_shadingMode);
    	}

		private void ChangePrimitive()
		{
			((ContentPreviewToolWindowControl)base.Content).ChangePrimitive(_primitive);
		}

		private void ShowNormals()
		{
			((ContentPreviewToolWindowControl)base.Content).ShowNormals(_normals.Checked);
		}

		private void ShowBoundingBox()
		{
			((ContentPreviewToolWindowControl)base.Content).ShowBoundingBox(_boundingBox.Checked);
		}

		private void ToggleAlphaBlend()
		{
			((ContentPreviewToolWindowControl)base.Content).ToggleAlphaBlend(_alphaBlend.Checked);
		}

        private void ToggleTextureSize()
        {
            ((ContentPreviewToolWindowControl)base.Content).ToggleTextureSize(_textureSize.Checked);
        }
    }

	public enum ShadingMode
	{
		Solid,
		Wireframe,
		SolidAndWireframe
	}

	public enum Primitive
	{
		Sphere,
		Cube,
		Cylinder,
		Torus,
		Plane,
		Teapot
	}
}
